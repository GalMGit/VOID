using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using DynamicData;
using Microsoft.AspNetCore.SignalR.Client;
using VOID.Shared.Contracts.Enums.Messages;
using VOID.Shared.Contracts.SignalRTokens;
using ReactiveUI;
using VOID.APP.Models.Group;
using VOID.APP.Models.Messages;
using VOID.APP.Models.Navigation;

namespace VOID.APP.ViewModels.Pages.Group.CurrentGroup;

public partial class CurrentGroupViewModel
{
    private void SetupSignalRHandlers()
    {
        AddHubSubscription(HubConnection.On<MessageModel>(SignalRTokens.NewGroupMessage, HandleMessageEvent));
        AddHubSubscription(HubConnection.On<Guid, Guid>(SignalRTokens.GroupMessageDeleted, HandleMessageDelete));
        AddHubSubscription(HubConnection.On<MessageModel, Guid>(SignalRTokens.MessageInGroupUpdated, HandleMessageInGroupUpdated));
        AddHubSubscription(HubConnection.On<Guid>(SignalRTokens.GroupMessagesRead, HandleGroupMessagesRead));
        AddHubSubscription(HubConnection.On<Guid, Guid>(SignalRTokens.UserLeaveFromGroup, HandleUserLeave));
        AddHubSubscription(HubConnection.On<Guid, Guid>(SignalRTokens.DeleteGroupMember, HandleDeleteGroupMember));
        AddHubSubscription(HubConnection.On<Guid, string>(SignalRTokens.GroupImageUpdated, HandleGroupImageUpdated));
    }

    private async Task HandleGroupImageUpdated(
        Guid groupId,
        string imageUrl)
    {
        if (CurrentGroup.Id != groupId) return;

        await Dispatcher.UIThread.InvokeAsync(() =>
            CurrentGroup.ImageUrl = imageUrl);
    }

    private async Task HandleDeleteGroupMember(
        Guid groupId,
        Guid memberId)
    {
        if (CurrentGroup.Id != groupId) return;

        var memberForDelete = CurrentGroup.Members
            .FirstOrDefault(x => x.MemberId == memberId);

        if (memberForDelete is null) return;

        await Dispatcher.UIThread.InvokeAsync(() =>
            CurrentGroup.Members.Remove(memberForDelete));
    }

    private async Task HandleUserLeave(
        Guid memberId,
        Guid groupId)
    {
        if (CurrentGroup.Id != groupId) return;

        var memberForLeave = CurrentGroup.Members
            .FirstOrDefault(x => x.MemberId == memberId);

        if (memberForLeave is null) return;

        await Dispatcher.UIThread.InvokeAsync(() =>
            CurrentGroup.Members.Remove(memberForLeave));
    }

    private async Task HandleGroupMessagesRead(Guid groupId)
    {
        if (CurrentGroup.Id != groupId) return;

        var unreadMessages = Messages
            .Where(x => x is { IsMine: true, IsRead: false })
            .ToList();

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (unreadMessages.Any())
                foreach (var message in unreadMessages)
                    message.IsRead = true;
        });
    }

    private async Task HandleMessageInGroupUpdated(
        MessageModel message,
        Guid groupId)
    {
        if (CurrentGroup.Id != groupId)
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

    private async Task HandleMessageDelete(Guid groupId, Guid messageId)
    {
        if (CurrentGroup.Id != groupId) return;

        var messageForDelete = Messages.FirstOrDefault(x =>
            x.Id == messageId);

        if (messageForDelete is null) return;

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            MessageCache.Remove(messageForDelete.Id);
            Messages.Remove(messageForDelete);
            CurrentGroup.MessageCount--;
        });
    }

    private async Task HandleMessageEvent(MessageModel @event)
    {
        if (CurrentGroup.Id != @event.ParentId) return;

        if (MessageCache.ContainsKey(@event.Id))
            return;

        @event.IsMine = @event.SenderId == CurrentUserId;
        MessageCache[@event.Id] = @event;
        CurrentGroup.IsCleared = false;

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
            CurrentGroup.MessageCount++;

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
                        SignalREvents.SendGroupMessagesReadEvent,
                        CurrentGroup.Id);
                }
            }
        });
    }

    private void SetupMessages()
    {
        MessageBus.Current.Listen<bool>(MessageTokens.GroupJoined)
            .Subscribe(x => IsGroupJoined = x)
            .DisposeWith(_disposables);

        MessageBus.Current.Listen<Unit>(MessageTokens.LoadGroupMessages)
            .SelectMany(_ => Observable.FromAsync(async () =>
            {
                if (MessageCache.Count == 0)
                    await LoadMessagesAsync(1);
            }))
            .Subscribe()
            .DisposeWith(_disposables);

        MessageBus.Current.Listen<List<GroupMemberModel>>(MessageTokens.MembersAdded)
            .SelectMany(m => Observable.FromAsync(async () =>
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                    CurrentGroup.Members.AddRange(m));
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

        MessageBus.Current.Listen<Unit>(MessageTokens.SendIsReadGroupMessages)
            .Throttle(TimeSpan.FromMilliseconds(200))
            .Where(_ => _isActive)
            .SelectMany(_ => Observable.FromAsync(async () =>
            {
                var lastUnreadMessage = Messages.LastOrDefault(x =>
                    !x.IsRead && x.SenderId != CurrentUserId);

                if (lastUnreadMessage is null) return;

                var unreadMessages = Messages.Where(x =>
                        !x.IsRead
                        && x.SenderId != CurrentUserId)
                    .ToList();

                if (unreadMessages.Any())
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        foreach (var message in unreadMessages)
                            message.IsRead = true;
                    });

                    await HubConnection.InvokeAsync(
                        SignalREvents.SendGroupMessagesReadEvent,
                        CurrentGroup.Id);
                }
            }))
            .Subscribe()
            .DisposeWith(_disposables);
    }
}