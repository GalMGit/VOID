using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using DynamicData.Binding;
using Microsoft.AspNetCore.SignalR.Client;
using VOID.Shared.Contracts.Enums.Messages;
using VOID.Shared.Contracts.SignalRTokens;
using ReactiveUI;
using VOID.APP.Models.Messages;
using VOID.APP.Models.Navigation;

namespace VOID.APP.ViewModels.Pages.Chat.CurrentChat;

public partial class CurrentChatViewModel
{
    private void SetupSignalRHandlers()
    {
       AddHubSubscription(HubConnection.On<MessageModel>(SignalRTokens.NewMessage, HandleMessageEvent)); ;
       AddHubSubscription(HubConnection.On<Guid, Guid>(SignalRTokens.MessageDeleted, HandleMessageDelete));
       AddHubSubscription(HubConnection.On<Guid, string>(SignalRTokens.AvatarUpdated, HandleAvatarUpdated));
       AddHubSubscription(HubConnection.On<Guid, bool>(SignalRTokens.UserStatusChanged, HandleOnlineStatusChanged));
       AddHubSubscription(HubConnection.On<Guid>(SignalRTokens.ChatCleared, HandleChatCleared));
       AddHubSubscription(HubConnection.On<Guid, bool>(SignalRTokens.Typing, HandleInterlocutorTyping));
       AddHubSubscription(HubConnection.On<Guid>(SignalRTokens.MessagesRead, HandleMessagesRead));
       AddHubSubscription(HubConnection.On<MessageModel, Guid>(SignalRTokens.MessageUpdated, HandleMessagesUpdated));
    }

    private async Task HandleMessagesUpdated(
        MessageModel message, 
        Guid chatId)
    {
        if (CurrentChat.Id != chatId)
            return;

        var messageForUpdate = Messages
            .FirstOrDefault(x => x.Id == message.Id);

        if (messageForUpdate is null) return;

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            messageForUpdate.Text = message.Text;
            messageForUpdate.IsEdited = true;

            var index = Messages.IndexOf(messageForUpdate);
            if (index >= 0)
                Messages[index] = messageForUpdate;
        });
    }

    private async Task HandleChatCleared(Guid chatId)
    {
        if (CurrentChat.Id != chatId) return;
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Messages.Clear();
            CurrentChat.IsCleared = true;
            CurrentChat.MessageCount = 0;
        });
    }

    private async Task HandleMessagesRead(Guid chatId)
    {
        if (CurrentChat.Id != chatId) return;

        var unreadMessages = Messages
            .Where(x => x is { IsMine: true, IsRead: false })
            .ToList();

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (!unreadMessages.Any()) return;
            foreach (var message in unreadMessages)
                message.IsRead = true;
        });
    }

    private async Task HandleOnlineStatusChanged(
        Guid userId, 
        bool isOnline)
    {
        if (CurrentChat.InterlocutorId != userId) return;

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            CurrentChat.InterlocutorOnline = isOnline;
            CurrentChat.InterlocutorLastSeen = DateTime.UtcNow.ToLocalTime();
        });
    }

    private async Task HandleAvatarUpdated(
        Guid userId, 
        string? imageUrl)
    {
        if (CurrentChat.InterlocutorId != userId) return;

        await Dispatcher.UIThread.InvokeAsync(() =>
            CurrentChat.ImageUrl = imageUrl);
    }

    private async Task HandleMessageDelete(
        Guid chatId, 
        Guid messageId)
    {
        if (CurrentChat.Id != chatId) return;

        var messageForDelete = Messages
            .FirstOrDefault(x => x.Id == messageId);

        if (messageForDelete is null) return;

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            MessageCache.Remove(messageForDelete.Id);
            Messages.Remove(messageForDelete);
            CurrentChat.MessageCount--;
        });
    }

    private async Task HandleInterlocutorTyping(
        Guid userId, 
        bool isTyping)
    {
        if (CurrentChat.InterlocutorId != userId) return;

        await Dispatcher.UIThread.InvokeAsync(() =>
            CurrentChat.InterlocutorIsTyping = isTyping);
    }

    private async Task HandleMessageEvent(MessageModel @event)
    {
        if (CurrentChat.Id != @event.ParentId) return;

        if (MessageCache.ContainsKey(@event.Id))
            return;

        @event.IsMine = @event.SenderId == CurrentUserId;
        @event.CreatedAt = @event.CreatedAt.ToLocalTime();
        MessageCache[@event.Id] = @event;
        CurrentChat.IsCleared = false;

            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                switch (@event.MessageType)
                {
                    case MessageType.Video:
                        @event.VideoUrl = @event.MediaUrl;
                        @event.VideoThumbnailUrl = @event.ThumbnailUrl;
                        @event.MediaUrl = null;
                        @event.ThumbnailUrl = null;
                        break;
                    case MessageType.Audio:
                        @event.AudioUrl = @event.MediaUrl;
                        @event.MediaUrl = null;
                        break;
                    case MessageType.Image:
                        @event.ImageUrl = @event.MediaUrl;
                        @event.ImageThumbnailUrl = @event.ThumbnailUrl;
                        @event.MediaUrl = null;
                        @event.ThumbnailUrl = null;
                        break;
                    case MessageType.Gif:
                        @event.GifUrl = @event.MediaUrl;
                        @event.MediaUrl = null;
                        break;
                }
                
                Messages.Add(@event);
                
                CurrentChat.MessageCount++;
                
                if (_isActive && @event.SenderId != CurrentUserId)
                {
                    var scrollInfo = new ScrollInfoRequest();
                    
                    MessageBus.Current.SendMessage(
                        scrollInfo, 
                        MessageTokens.GetScrollPosition);
                    
                    if (scrollInfo.IsNearBottom)
                    {
                        await Task.Delay(100);
                        
                        MessageBus.Current.SendMessage(
                            Unit.Default, 
                            MessageTokens.ScrollToBottom);
                        
                        await HubConnection.InvokeAsync(
                            SignalREvents.SendMessagesReadEvent,
                            CurrentChat.InterlocutorId,
                            CurrentChat.Id);
                    }
                }
            });
    }

    private void SetupMessages()
    {
        MessageBus.Current.Listen<Unit>(MessageTokens.LoadMessages)
            .SelectMany(_ => Observable.FromAsync(async () =>
                {
                    if (MessageCache.Count == 0)
                        await LoadMessagesAsync(1);
                }))
            .Subscribe()
            .DisposeWith(_disposables);

        this.WhenAnyValue(x => x.MessageText)
            .Skip(1)
            .Publish(shared =>
                Observable.Merge(
                    shared.Select(_ => true),
                    shared.Throttle(TimeSpan.FromSeconds(2))
                        .Select(_ => false)
                ))
            .DistinctUntilChanged()
            .SelectMany(isTyping =>
                Observable.FromAsync(() =>
                    HubConnection.InvokeAsync(
                        SignalREvents.SendTypingEvent,
                        CurrentChat.InterlocutorId,
                        isTyping)))
            .Subscribe();

        MessageBus.Current.Listen<Unit>(MessageTokens.SendIsReadMessages)
            .Throttle(TimeSpan.FromMilliseconds(200))
            .Where(_ => _isActive)
            .SelectMany(_ => Observable.FromAsync(async () =>
            {
                var lastMessage = Messages.LastOrDefault();
                if (lastMessage is null) return;

                if (!lastMessage.IsRead && lastMessage.SenderId != CurrentUserId)
                {
                    if (CurrentChat.InterlocutorId != CurrentUserId)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                            lastMessage.IsRead = true);

                        await HubConnection.InvokeAsync(
                            SignalREvents.SendMessagesReadEvent,
                            CurrentChat.InterlocutorId,
                            CurrentChat.Id);
                    }
                }
            }))
            .Subscribe()
            .DisposeWith(_disposables);

        MessageBus.Current.Listen<Unit>(MessageTokens.MessagesLoadNextPage)
                .Throttle(TimeSpan.FromMilliseconds(50))
                .Where(_ => _isActive)
                .SelectMany(_ => Observable.FromAsync(async () =>
                    await LoadNextPage()))
                .Subscribe()
                .DisposeWith(_disposables);
    }
}

public sealed record UnreadCountEvent(int Count, Guid ChatId);