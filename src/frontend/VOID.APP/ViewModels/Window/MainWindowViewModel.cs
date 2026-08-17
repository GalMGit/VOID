using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Styling;
using Avalonia.Threading;
using Microsoft.AspNetCore.SignalR.Client;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using VOID.APP.Models.Navigation;
using VOID.APP.Models.User;
using VOID.APP.Services.Interfaces;
using VOID.APP.Services.Interfaces.IAuth;
using VOID.APP.Services.Interfaces.ISettings;
using VOID.APP.ViewModels.Base.PageBase;
using VOID.APP.ViewModels.Pages.Auth.AuthLayout;

namespace VOID.APP.ViewModels.Window;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ITokenService _tokenService;
    private readonly IAuthService _authService;
    private readonly ISettingsService _settingsService;
    private readonly HubConnection _hubConnection;
    private readonly IViewModelFactory _viewModelFactory;
    [Reactive] public partial PageViewModelBase CurrentPage { get; set; }

    public MainWindowViewModel(
        HubConnection hubConnection,
        IViewModelFactory viewModelFactory,
        ITokenService tokenService,
        IAuthService authService,
        ISettingsService settingsService)
    {
        _hubConnection = hubConnection;
        _settingsService = settingsService;

        _viewModelFactory = viewModelFactory;
        _tokenService = tokenService;
        _authService = authService;
        _ = ApplySavedThemeAsync();
        CurrentPage = _viewModelFactory.CreateAuthLayout();
        
        SetupLogoutListener();

        SetupMessages();
        Task.Run(async () => await InitializeAuth());
    }
    
    private void SetupLogoutListener()
    {
        MessageBus.Current.Listen<Unit>(MessageTokens.ErrorAuthLogout)
            .Subscribe(_ => CurrentPage = _viewModelFactory.CreateAuthLayout());
    }

    private async Task InitializeAuth()
    {
        await _tokenService.LoadTokensAsync();

        if (string.IsNullOrWhiteSpace(_tokenService.RefreshToken))
        {
            CurrentPage = _viewModelFactory.CreateAuthLayout();

            return;
        }

        if (!_tokenService.IsTokenValid())
        {
            var refreshed = await _authService.RefreshTokenAsync();

            if (!refreshed)
            {
                await _tokenService.ClearStoredTokenAsync();

                CurrentPage = _viewModelFactory.CreateAuthLayout();
                return;
            }
        }

        var userInfo = _tokenService.GetUserInfoFromToken();

        if (!userInfo.IsAuthenticated)
        {
            CurrentPage = _viewModelFactory.CreateAuthLayout();
            return;
        }

        await StartHubConnectionAsync();

        if (CurrentPage is AuthLayoutViewModel auth)
            auth.Dispose();

        CurrentPage = _viewModelFactory.CreateLayout(
                new UserSession(
                    userInfo.Username,
                    userInfo.Id));

        MessageBus.Current.SendMessage(
            Unit.Default,
            MessageTokens.LoadAvatars);
    }

    private async Task ApplySavedThemeAsync()
    {
        var settings = await _settingsService.LoadSettingsAsync();

        await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (Avalonia.Application.Current != null)
                {
                    Avalonia.Application.Current.RequestedThemeVariant =
                        settings.Theme == "Light"
                            ? ThemeVariant.Light
                            : ThemeVariant.Dark;
                }
            });
    }

    private void SetupMessages()
    {
        MessageBus.Current.Listen<AuthUser>(MessageTokens.GoToMain)
            .SelectMany(user => Observable.FromAsync(async () =>
                    await StartHubConnectionAsync())
                .Do(_ =>
                {
                    if (CurrentPage is AuthLayoutViewModel auth)
                        auth.Dispose();

                    var session = new UserSession(
                        user.Username,
                        user.Id);

                    CurrentPage = _viewModelFactory.CreateLayout(session);
                    
                    MessageBus.Current.SendMessage(
                        Unit.Default,
                        MessageTokens.LoadAvatars);
                }))
            .Subscribe();

        MessageBus.Current.Listen<Unit>(MessageTokens.Logout)
            .SelectMany(_ => Observable.FromAsync(HandleLogout))
            .Subscribe();
    }
}