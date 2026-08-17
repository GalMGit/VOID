using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Microsoft.AspNetCore.SignalR.Client;
using MsBox.Avalonia;
using VOID.APP.Services.Interfaces.IGroup;
using VOID.Shared.Contracts.Enums.Chats;
using VOID.Shared.Contracts.Enums.Messages;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using VOID.APP.Models.Group;
using VOID.APP.Models.Messages;
using VOID.APP.Models.Navigation;
using VOID.APP.Models.User;
using VOID.APP.Services.Interfaces.IAudio;
using VOID.APP.Services.Interfaces.IFile;
using VOID.APP.Services.Interfaces.IMessage;
using VOID.APP.ViewModels.Pages.Base.ChatBase;

namespace VOID.APP.ViewModels.Pages.Group.CurrentGroup;

public partial class CurrentGroupViewModel : BaseChatViewModel
{
    private bool _isActive;
    [Reactive] public partial FullGroupModel CurrentGroup { get; set; }
    [Reactive] public partial bool IsGroupJoined { get; set; }

    public CurrentGroupViewModel(
        UserSession userSession,
        FullGroupModel currentGroup,
        HubConnection hubConnection,
        IMessageService messageService,
        IFilePickerService filePickerService,
        IAudioRecordingService audioRecordingService,
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
        CurrentGroup = currentGroup;
        CurrentGroup.CurrentUserId = userSession.UserId;
        SetupMessages();
        SetupSignalRHandlers();
    }

    [ReactiveCommand]
    private void OpenEditGroup()
        => MessageBus.Current.SendMessage(
            CurrentGroup, 
            MessageTokens.OpenEditGroupDialog);

    public async Task JoinToGroupAsync(Guid groupId)
        => await HubConnection.InvokeAsync(
            SignalREvents.JoinToGroupEvent, 
            groupId);
    
    [ReactiveCommand]
    private void AddMember()
        => MessageBus.Current.SendMessage(new GroupModel 
            {
                Id = CurrentGroup.Id,
                ChatName = CurrentGroup.ChatName,
                ImageUrl = CurrentGroup.ImageUrl,
                CreatedAt = CurrentGroup.CreatedAt,
                OwnerId = CurrentGroup.OwnerId 
            },
            MessageTokens.OpenAddMemberDialog);

    protected override Guid GetChatId() 
        => CurrentGroup.Id;
    
    protected override ChatType GetChatType() 
        => ChatType.Group;

    protected override async Task SendMessageAsync()
    {
        var text = MessageText.Trim();
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
            CurrentGroup.Id,
            MessageType.Text,
            ChatType.Group);

        await Dispatcher.UIThread.InvokeAsync(() 
            => MessageText = string.Empty);

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
        => await MessageService.CreateMessageAsync(
            null,
            stream,
            fileName,
            CurrentGroup.Id,
            MessageType.Gif,
            ChatType.Group,
            ct: ct);

    protected override async Task SendMediaMessageAsync(
        Stream stream, 
        string fileName, 
        MessageType messageType,
        IProgress<long>? progress = null, 
        CancellationToken ct = default)
        => await MessageService.CreateMessageAsync(
            null,
            stream,
            fileName,
            CurrentGroup.Id,
            messageType,
            ChatType.Group, ct: ct);

    protected override async Task DeleteMessageAsync(MessageModel message)
        => await MessageService.HardMessageDeleteAsync(message.Id);
    
    public async Task Activate()
    {
        _isActive = true;
        
        var unreadMessages = Messages
            .Where(x => x is { IsMine: false, IsRead: false })
            .ToList();
        
        if (unreadMessages.Any())
        {
            await HubConnection.InvokeAsync(
                SignalREvents.SendGroupMessagesReadEvent,
                CurrentGroup.Id);
        }
    }

    public void Deactivate()
        => _isActive = false;
}