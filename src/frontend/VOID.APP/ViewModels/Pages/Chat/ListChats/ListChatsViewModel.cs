using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using VOID.APP.Extensions;
using VOID.APP.Services.Interfaces.IMessage;
using VOID.APP.Services.Interfaces.INotify;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using VOID.APP.Models.Chat;
using VOID.APP.Models.Navigation;
using VOID.APP.Models.User;
using VOID.APP.Services.Interfaces;
using VOID.APP.Services.Interfaces.IChat;
using VOID.APP.ViewModels.Base.PageBase;
using VOID.APP.ViewModels.Pages.Chat.CurrentChat;

namespace VOID.APP.ViewModels.Pages.Chat.ListChats;

public partial class ListChatsViewModel : PageViewModelBase
{
    private readonly Guid _currentUserId;
    private readonly UserSession _userSession;
    private readonly IChatService _chatService;
    private readonly HubConnection _hubConnection;
    private readonly Dictionary<Guid, CurrentChatViewModel> _chatCache = [];
    private CurrentChatViewModel? _currentActiveChatVm;
    private readonly IViewModelFactory _viewModelFactory;
    private readonly INotificationService _notificationService;
    [Reactive] public partial ChatModel? SelectedChatItem { get; set; }
    public ObservableCollection<ChatModel> Chats { get; set; } = [];
    public ReactiveCommand<Unit,Unit> ClearSelectionCommand { get; }
    private readonly List<IDisposable> _hubSubscriptions = [];

    private int _currentPage;
    private bool _hasNextPages;
    private int _totalPages;
    private bool _isRefreshing;
    private bool _isLoadingMore;
    private const int PageSize = 15;

    public ListChatsViewModel(
        UserSession userSession,
        IChatService chatService,
        HubConnection hubConnection,
        IViewModelFactory viewModelFactory,
        INotificationService notificationService
        )
    {
        Title = "Чаты";
        _userSession = userSession;
        _chatService = chatService;
        _currentUserId = userSession.UserId;
        _viewModelFactory = viewModelFactory;
        _hubConnection = hubConnection;
        _notificationService = notificationService;

        ClearSelectionCommand = ReactiveCommand.Create(() =>
        {
            SelectedChatItem = null;
            _currentActiveChatVm?.Deactivate();
            _currentActiveChatVm = null;
            
            MessageBus.Current.SendMessage(
                Unit.Default,
                MessageTokens.ClearSelectionCurrent);
            
        }).DisposeWith(_disposables);

        SetupSignalRHandlers();
        SetupMessages();
    }

    [ReactiveCommand]
    private async Task ClearChatAsync(ChatModel chat)
    {
        if (chat.LastMessageDate is null) return;
        await _chatService.ClearChatAsync(chat.Id);
    }

    private async Task<FullChatModel?> LoadChatAsync(Guid chatId)
        => await _chatService.GetChatByIdAsync(chatId);

    private async Task LoadChats(int pageNumber)
    {
        if (_isLoadingMore) return;
        _isLoadingMore = true;
        try
        {
            var result = await _chatService.GetChatsForUserAsync(
                pageNumber,
                PageSize);

            if (result is not null)
            {
                if (pageNumber == 1)
                {
                    foreach (var chat in result.Items)
                    {
                        Chats.Add(chat);
                    }
                }
                else
                {
                    foreach (var chat in result.Items
                                 .Where(chat => Chats
                                     .All(x => x.Id != chat.Id)))
                    {
                        Chats.Add(chat);
                    }
                }

                _currentPage = result.PageNumber;
                _totalPages = result.TotalPages;
                _hasNextPages = result.HasNext;
            }
        }
        finally
        {
            _isLoadingMore = false;
        }
    }

    private async Task LoadNextPage()
    {
        if(_hasNextPages && !_isLoadingMore && !_isRefreshing)
            await LoadChats(_currentPage + 1);
    }

    private void CleanChatCache(Guid chatId)
    {
        if (!_chatCache.TryGetValue(chatId, out var chatVm))
            return;

        _chatCache.Remove(chatId);

        chatVm.Dispose();

        MessageBus.Current.SendMessage(
            Unit.Default, 
            MessageTokens.ClearSelectionCurrent);
    }

    [ReactiveCommand]
    private async Task HardDeleteChatAsync(ChatModel chat)
    {
        await _chatService.HardDeleteChatAsync(chat.Id);
        MessageBus.Current.SendMessage(
            Unit.Default,
            MessageTokens.ClearLastSearchText);
    }

    private async Task CreateChatAsync(
        SearchUserResponse user, 
        CancellationToken ct = default)
        => await _chatService.CreateChatAsync(
            user.Username, ct);
}