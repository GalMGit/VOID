using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using Microsoft.AspNetCore.SignalR.Client;
using VOID.APP.ViewModels.Pages.Chat.CurrentChat;
using VOID.Shared.Contracts.Enums.Messages;
using VOID.Shared.Contracts.SignalRTokens;
using ReactiveUI;
using VOID.APP.Models.Chat;
using VOID.APP.Models.Messages;
using VOID.APP.Models.Navigation;
using VOID.APP.Models.User;

namespace VOID.APP.ViewModels.Pages.Chat.ListChats;

public partial class ListChatsViewModel
{
    private void SetupSignalRHandlers()
    {
        _hubSubscriptions.Add(_hubConnection.On<ChatModel>(SignalRTokens.PrivateChatCreated, HandlePrivateChatCreated));
        _hubSubscriptions.Add(_hubConnection.On<Guid>(SignalRTokens.ChatDeleted, HandlePrivateChatDeleted));
        _hubSubscriptions.Add(_hubConnection.On<MessageModel>(SignalRTokens.NewMessage, HandleLastMessageChanged));
        _hubSubscriptions.Add(_hubConnection.On<Guid, string>(SignalRTokens.AvatarUpdated, HandleAvatarUpdated));
        _hubSubscriptions.Add(_hubConnection.On<Guid, bool>(SignalRTokens.UserStatusChanged, HandleOnlineStatusChanged));
        _hubSubscriptions.Add(_hubConnection.On<Guid, Guid, MessageModel>(SignalRTokens.MessageDeleted, HandleMessageDeleted));
        _hubSubscriptions.Add(_hubConnection.On<Guid>(SignalRTokens.ChatCleared, HandleChatCleared));
        _hubSubscriptions.Add(_hubConnection.On<Guid, bool>(SignalRTokens.Typing, HandleInterlocutorTyping));
        _hubSubscriptions.Add(_hubConnection.On<string, Guid>(SignalRTokens.UserNameUpdated, HandleUserNameUpdated));
        _hubSubscriptions.Add(_hubConnection.On<MessageModel, Guid>(SignalRTokens.MessageUpdated, HandleMessageUpdated));
    }

    private async Task HandleMessageUpdated(MessageModel message, Guid chatId)
    {
        var chat = Chats
            .FirstOrDefault(x => x.Id == chatId);

        if (chat is null) return;

        await Dispatcher.UIThread.InvokeAsync(() =>
            chat.LastMessage = message.Text);
    }

    private async Task HandleUserNameUpdated(
        string name, 
        Guid userId)
    {
        var chat = Chats
            .FirstOrDefault(x => x.InterlocutorId == userId);

        if (chat is null) return;

        await Dispatcher.UIThread.InvokeAsync(() =>
            chat.ChatName = name);
    }

    private async Task HandleMessageDeleted(
        Guid chatId,
        Guid messageId, 
        MessageModel lastMessage)
    {
        var chat = Chats
            .FirstOrDefault(x => x.Id == chatId);

        if (chat is null)
            return;
        
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            chat.LastMessage = lastMessage?.Text ?? "Нет сообщений";
            chat.LastMessageDate = lastMessage?.CreatedAt;
        });

    }

    private async Task HandleOnlineStatusChanged(Guid userId, bool isOnline)
    {
        var chat = Chats
            .FirstOrDefault(x => x.InterlocutorId == userId);
        
        if(chat is null) return;
        
        await Dispatcher.UIThread.InvokeAsync(() =>
            chat.InterlocutorOnline = isOnline);
    }

    private async Task HandleChatCleared(Guid chatId)
    {
        var chat = Chats
            .FirstOrDefault(x => x.Id == chatId);
        
        if (chat is null) return;

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            chat.LastMessage = "История очищена";
            chat.LastMessageDate = null;
        });
    }

    private async Task HandleAvatarUpdated(
        Guid userId, 
        string? imageUrl)
    {
        var chatForUpdate = Chats
            .FirstOrDefault(x => x.InterlocutorId == userId);
        
        if (chatForUpdate is null)
            return;
        
        Console.WriteLine(imageUrl);

        await Dispatcher.UIThread.InvokeAsync(() =>
            chatForUpdate.ImageUrl = imageUrl);
    }

    private async Task HandlePrivateChatDeleted(Guid chatId)
    {
        var chatForRemove = Chats
            .FirstOrDefault(x => x.Id == chatId);
        
        if (chatForRemove is null) return;

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Chats.Remove(chatForRemove);
            CleanChatCache(chatId);
        });
    }

    private async Task HandleInterlocutorTyping(
        Guid userId, 
        bool isTyping)
    {
        var chat = Chats
            .FirstOrDefault(x => x.InterlocutorId == userId);
        
        if (chat is null) return;

        await Dispatcher.UIThread.InvokeAsync(() =>
            chat.InterlocutorTyping = isTyping);
    }

    private async Task HandlePrivateChatCreated(ChatModel @event)
    {
        if (Chats.Any(x => x.Id == @event.Id)) 
            return;

        await Dispatcher.UIThread.InvokeAsync(() =>
            Chats.Insert(0, @event));
    }

    private async Task HandleLastMessageChanged(MessageModel @event)
    {
        var chat = Chats
            .FirstOrDefault(x => x.Id == @event.ParentId);
        
        if (chat is null) return;

        var isThisChatActive = _currentActiveChatVm?.CurrentChat.Id == chat.Id;

        if (!isThisChatActive && @event.SenderId != _currentUserId)
        {
            var shortMessage = @event.Text.Length > 50
                ? @event.Text[..50] + "..."
                : @event.Text;
            
            await _notificationService.ShowNotificationAsync(
                $"📩 {chat.ChatName}",
                shortMessage
            );
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var lastMessage = @event.MessageType switch
            {
                MessageType.Image => "Фото",
                MessageType.Audio => "Голосовое",
                MessageType.Video => "Видео",
                _ => FormatLastMessage(@event.Text)
            };

            chat.LastMessage = lastMessage;
            chat.LastMessageDate = @event.CreatedAt.ToLocalTime();
            
            if (!isThisChatActive && @event.SenderId != _currentUserId)
            {
                chat.UnreadCount++;
                Console.WriteLine(chat.UnreadCount);
            }
            
            var currentIndex = Chats.IndexOf(chat);
            
            if (currentIndex > 0)
                Chats.Move(currentIndex, 0);
        });
    }

    private string FormatLastMessage(
        string text)
    {
        return string.IsNullOrEmpty(text)
            ? "Нет сообщений"
            : text;
    }

    private void SetupMessages()
    {
        MessageBus.Current.Listen<Unit>(MessageTokens.LoadLists)
            .SelectMany(_ => Observable.FromAsync(async () =>
            {
                await Task.Delay(100);
                await LoadChats(1);
            }))
            .Subscribe()
            .DisposeWith(_disposables);

        MessageBus.Current.Listen<SearchUserResponse>(MessageTokens.CreateChat)
            .SelectMany(u => Observable.FromAsync(async () =>
            {
                await OpenOrCreateChatAsync(u);
            }))
            .Subscribe()
            .DisposeWith(_disposables);

        MessageBus.Current.Listen<Guid>("DumpUnreadCount")
            .Subscribe(c =>
            {
               var chat = Chats
                    .FirstOrDefault(x => x.Id == c);

               if (chat is not null)
                   Dispatcher.UIThread.InvokeAsync(() =>
                       chat.UnreadCount = 0);
            })
            .DisposeWith(_disposables);

        this.WhenAnyValue(x => x.SelectedChatItem)
            .WhereNotNull()
            .SelectMany(c => Observable.FromAsync(async () =>
            {
                _currentActiveChatVm?.Deactivate();

                if (!_chatCache.TryGetValue(c.Id, out var vm))
                {
                    var chat = await LoadChatAsync(c.Id);

                    vm = _viewModelFactory.CreateChat(
                        _userSession, 
                        chat!);
                    
                    _chatCache.Add(
                        c.Id, 
                        vm);
                    
                    MessageBus.Current.SendMessage(
                        Unit.Default, 
                        MessageTokens.LoadMessages);
                }

                _currentActiveChatVm = vm;
                
                MessageBus.Current.SendMessage(
                    vm, 
                    MessageTokens.ChatPicked);
                
                await vm.Activate();
                MessageBus.Current.SendMessage(
                    c.Id,
                    "DumpUnreadCount");
            }))
            .Subscribe()
            .DisposeWith(_disposables);

        MessageBus.Current.Listen<Unit>(MessageTokens.ClearSelectionList)
            .Subscribe(_ =>
            {
                _currentActiveChatVm?.Deactivate();
                SelectedChatItem = null;
            })
            .DisposeWith(_disposables);
        
        MessageBus.Current.Listen<Unit>(MessageTokens.ClearSelectionCurrent)
            .Subscribe(_ =>
            {
                _currentActiveChatVm?.Deactivate();
                _currentActiveChatVm = null;
                SelectedChatItem = null;
            })
            .DisposeWith(_disposables);

        MessageBus.Current.Listen<Unit>(MessageTokens.ChatsLoadNextPage)
            .SelectMany(_ => Observable.FromAsync(async () =>
                await LoadNextPage()))
            .Subscribe()
            .DisposeWith(_disposables);
    }

    private async Task OpenOrCreateChatAsync(SearchUserResponse user)
    {
        var existingChat = await _chatService.GetPrivateChatWithUserAsync(user.Id);

        if (existingChat is not null)
        {
            await OpenChatAsync(existingChat.Id);
            return;
        }

        await CreateChatAsync(user);
    }

    private async Task OpenChatAsync(Guid chatId)
    {
        if (!_chatCache.TryGetValue(chatId, out var chatVm))
        {
            var chat = await LoadChatAsync(chatId);

            if (chat is null)
                return;

            chatVm = _viewModelFactory.CreateChat(
                _userSession,
                chat);
            
            _chatCache[chatId] = chatVm;
            
            MessageBus.Current.SendMessage(
                Unit.Default, 
                MessageTokens.LoadMessages);
        }

        _currentActiveChatVm?.Deactivate();

        _currentActiveChatVm = chatVm;
        
        MessageBus.Current.SendMessage(
            chatVm, 
            MessageTokens.ChatPicked);
        
        await chatVm.Activate();
    }
}