using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Threading;
using DynamicData;
using Microsoft.AspNetCore.SignalR.Client;
using MsBox.Avalonia;
using VOID.Shared.Contracts.Enums.Chats;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using VOID.APP.Models.Chat;
using VOID.APP.Models.Messages;
using VOID.APP.Models.Navigation;
using VOID.APP.Models.User;
using VOID.APP.Services.Interfaces.IAudio;
using VOID.APP.Services.Interfaces.IFile;
using VOID.APP.Services.Interfaces.IMessage;
using VOID.APP.ViewModels.Pages.Base.ChatBase;
using MessageType = VOID.Shared.Contracts.Enums.Messages.MessageType;

namespace VOID.APP.ViewModels.Pages.Chat.CurrentChat;

public partial class CurrentChatViewModel : BaseChatViewModel
{
    [Reactive] public partial FullChatModel CurrentChat { get; set; }
    public ReactiveCommand<Unit, Unit> OpenProfileDialogCommand { get; set; }
    public ReactiveCommand<Unit, Unit> ClearSelectionCommand { get; }
    [Reactive] public partial bool IsMessageBoxNotEmpty { get; set; }
    public ObservableCollection<MessageModel> SelectedMessages { get; set; } = [];
    [Reactive] public partial string ErrorMessage { get; set; }

    private bool _isActive;

    public CurrentChatViewModel(
        UserSession userSession,
        FullChatModel currentChat,
        IMessageService messageService,
        HubConnection hubConnection,
        IAudioRecordingService audioRecordingService,
        IFilePickerService filePickerService,
        IAudioPlaybackService audioPlaybackService)
        : base(
            messageService, 
            hubConnection, 
            filePickerService, 
            userSession.UserId,
            audioRecordingService,
            audioPlaybackService
            )
    {
        CurrentChat = currentChat;
        ClearSelectionCommand = ReactiveCommand.Create(() =>
        {
            MessageBus.Current.SendMessage(
                Unit.Default,
                MessageTokens.ClearSelectionList);
            
            MessageBus.Current.SendMessage(
                Unit.Default,
                MessageTokens.ClearSelectionCurrent);
            
        }).DisposeWith(_disposables);

        OpenProfileDialogCommand = ReactiveCommand.Create(() =>
        {
            MessageBus.Current.SendMessage(
                CurrentChat, 
                MessageTokens.OpenInterlocutorProfile);
            
        }).DisposeWith(_disposables);

        SetupSignalRHandlers();
        SetupMessages();
    }

    protected override Guid GetChatId() 
        => CurrentChat.Id;
    
    protected override ChatType GetChatType() 
        => ChatType.Private;

    protected override async Task SendMessageAsync()
    {
        var text = MessageText?.Trim();
        if (string.IsNullOrWhiteSpace(text)) return;
        if (text.Length >= 4000)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Ошибка", 
                "Текст сообщения не должен превышать 4000 символов");
            
            await box.ShowAsync();
            return;
        }
        
        await MessageService.CreateMessageAsync(
            text,
            null,
            null,
            CurrentChat.Id,
            MessageType.Text,
            ChatType.Private);
        
        await Dispatcher.UIThread.InvokeAsync(() => 
            MessageText = string.Empty);
        
        await Task.Delay(100);
        
        MessageBus.Current.SendMessage(
            Unit.Default, 
            MessageTokens.ScrollToBottom);
    }
    
    protected override async Task SendGifMessageAsync(
        Stream stream, 
        string fileName,
        IProgress<long>? progress = null,
        CancellationToken ct = default)
    {
        await MessageService.CreateMessageAsync(
            null,
            stream,
            fileName,
            CurrentChat.Id,
            MessageType.Gif,
            ChatType.Private, ct: ct);
        
        MessageBus.Current.SendMessage(
            Unit.Default, 
            MessageTokens.ScrollToBottom);
    }

    protected override async Task SendMediaMessageAsync(
        Stream stream, 
        string fileName, 
        MessageType messageType,
        IProgress<long>? progress = null,
        CancellationToken ct = default)
    {
        await MessageService.CreateMessageAsync(
            null,
            stream,
            fileName,
            CurrentChat.Id,
            messageType,
            ChatType.Private, 
            progress, ct: ct);
        
        MessageBus.Current.SendMessage(
            Unit.Default, 
            MessageTokens.ScrollToBottom);
    }

    protected override async Task DeleteMessageAsync(MessageModel message)
        => await MessageService.HardMessageDeleteAsync(message.Id);

    [ReactiveCommand]
    private void ClearSelections(IList messages)
        => Dispatcher.UIThread.InvokeAsync(messages.Clear);
    

    [ReactiveCommand]
    private void DeleteSelections(IList messages)
    {
        List<MessageModel> items = [];
        foreach (var item in messages)
        {
            if (item is not MessageModel message) 
                continue;
            
            items.Add(message);

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                Messages.RemoveMany(items);
            });
        }
        MessageService.DeleteMessagesAsync(items
            .Select(x => x.Id)
            .ToList());
    }
    
    public async Task Activate()
    {
        _isActive = true;

        var unreadMessagesFromInterlocutor = Messages
            .Where(x => x is { IsMine: false, IsRead: false })
            .ToList();

        if (unreadMessagesFromInterlocutor.Any())
        {
            await HubConnection.InvokeAsync(
                SignalREvents.SendMessagesReadEvent,
                CurrentChat.InterlocutorId,
                CurrentChat.Id);
        }
    }

    public void Deactivate()
        => _isActive = false;
}