using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Styling;
using DialogHostAvalonia;
using Microsoft.AspNetCore.SignalR.Client;
using VOID.APP.Extensions;
using VOID.APP.Services.Interfaces.IImage;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using VOID.APP.Models.Navigation;
using VOID.APP.Models.User;
using VOID.APP.Services.Interfaces;
using VOID.APP.Services.Interfaces.IChat;
using VOID.APP.Services.Interfaces.ISettings;
using VOID.APP.ViewModels.Base.ModalBase;
using VOID.APP.ViewModels.Base.PageBase;
using VOID.APP.ViewModels.Pages.Layout.Search;

namespace VOID.APP.ViewModels.Pages.Layout;

public partial class LayoutViewModel : PageViewModelBase
{
    private readonly IChatService _chatService;
    private readonly IViewModelFactory _viewModelFactory;
    private readonly ISettingsService _settingsService;
    private readonly Guid _currentUserId;
    private readonly UserSession _userSession;
    private SearchListViewModel? _searchVm;
    private readonly IDialogService _dialogService;
    private readonly HubConnection _hubConnection;
    private readonly List<IDisposable> _hubSubscriptions = [];

    private string? _lastSearchText;

    [Reactive] public partial string? AvatarImage { get; set; }
    [Reactive] public partial string CurrentName { get; set; }
    [Reactive] public partial string SearchText { get; set; }
    [Reactive] public partial SearchUserResponse SelectedUser { get; set; }
    [Reactive] public partial PageViewModelBase? CurrentChatContent { get; set; }
    [Reactive] public partial PageViewModelBase? CurrentListContent { get; set; }
    [Reactive] public partial ModalViewModelBase? ProfileContent { get; set; }
    [Reactive] public partial ModalViewModelBase? CreateGroupContent { get; set; }
    [Reactive] public partial ModalViewModelBase? InterlocutorProfileContent { get; set; }
    [Reactive] public partial PageViewModelBase? SelectedListItem { get; set; }
    [Reactive] public partial bool IsThemeChecked { get; set; }
    [Reactive] public partial int TotalConnections { get; set; }
    public ObservableCollection<PageViewModelBase> ListPages { get; set; }

    private ObservableCollection<SearchUserResponse> SearchResults { get; set; } = [];
    public ReactiveCommand<Unit, Unit> OpenProfileDialogCommand { get; }
    public ReactiveCommand<Unit, Unit> SearchCommand { get; set; }
    public ReactiveCommand<Unit, Unit> OpenCreateGroupDialogCommand { get; }

    public LayoutViewModel(
        UserSession userSession,
        IChatService chatService,
        IViewModelFactory viewModelFactory,
        IDialogService dialogService,
        ISettingsService settingsService,
        HubConnection hubConnection)
    {
        _currentUserId = userSession.UserId;
        _chatService = chatService;
        _dialogService = dialogService;
        _userSession = userSession;
        _settingsService = settingsService;
        _hubConnection = hubConnection;
        ProfileContent = viewModelFactory.CreateProfile(userSession);

        _viewModelFactory = viewModelFactory;

        ListPages =
        [
            viewModelFactory.CreateListChats(userSession),
            viewModelFactory.CreateListGroups(userSession)
        ];
        
        SelectedListItem = ListPages[0];
        CurrentListContent = ListPages[0];

        OpenProfileDialogCommand = ReactiveCommand.CreateFromTask(async () =>
                await OpenDialogAsync(ProfileContent))
            .DisposeWith(_disposables);

        OpenCreateGroupDialogCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            CreateGroupContent ??= _viewModelFactory.CreateGroupModal();

            await OpenDialogAsync(CreateGroupContent);
        }).DisposeWith(_disposables);
        
        SearchCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (string.IsNullOrWhiteSpace(SearchText) 
                || SearchText == _lastSearchText) 
                return;

            _lastSearchText = SearchText;

            await FilterUsersAsync(SearchText);

            _searchVm ??= _viewModelFactory.CreateSearchList(SearchResults);
            CurrentListContent = _searchVm;
        });
        SetupSignalRHandlers();
        SetupMessages();
    }

    private async Task FilterUsersAsync(string searchTerm)
    {
        var users = await _chatService.GetSearchUsers(searchTerm);
        
        var filteredUsers = users
            .Where(u => u.Id != _currentUserId);
        
        await SearchResults.UpdateFromAsync(filteredUsers);
    }

    private async Task OpenDialogAsync(ModalViewModelBase content)
        => await _dialogService.ShowAsync(content);

    private void CloseDialog()
        => DialogHost.Close(DialogNames.Dialog);

    [ReactiveCommand]
    private async Task SwitchTheme()
    {
        if (Application.Current!.RequestedThemeVariant == ThemeVariant.Light)
        {
            Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
        }
        else if (Application.Current!.RequestedThemeVariant == ThemeVariant.Dark)
        {
            Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        }

        var settings = await _settingsService.LoadSettingsAsync();

        settings.Theme =
            Application.Current.RequestedThemeVariant == ThemeVariant.Light
                ? "Light"
                : "Dark";

        await _settingsService.SaveSettingsAsync(settings);
    }
}