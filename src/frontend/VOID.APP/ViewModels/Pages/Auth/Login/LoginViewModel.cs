using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using VOID.APP.Models.Navigation;
using VOID.APP.Services.Interfaces.IAuth;
using VOID.APP.ViewModels.Base.PageBase;

namespace VOID.APP.ViewModels.Pages.Auth.Login;

public partial class LoginViewModel : PageViewModelBase
{
    private readonly ITokenService _tokenService;
    private readonly IAuthService _authService;

    [Reactive] public partial string Username { get; set; }
    [Reactive] public partial string Password { get; set; }
    [Reactive] public partial string StatusMessage { get; set; }
    [Reactive] public partial bool ShowPassword { get; set; }

    public ReactiveCommand<Unit, Unit> GoToRegisterCommand { get; }

    public LoginViewModel(
        ITokenService tokenService,
        IAuthService authService)
    {
        _tokenService = tokenService;
        _authService = authService;

        GoToRegisterCommand = ReactiveCommand.Create(() =>
        {
            Username = string.Empty;
            Password = string.Empty;
            StatusMessage = string.Empty;
            ShowPassword = false;
            
            MessageBus.Current.SendMessage(
                Unit.Default,
                MessageTokens.GoToRegister);
            
        }).DisposeWith(_disposables);
    }
    
    [ReactiveCommand]
    private async Task LoginAsync()
    {
        try
        {
            var (success, errorMessage) = await _authService.LoginAsync(
                Username, 
                Password);

            if (success)
            {
                var userInfo = _tokenService.GetUserInfoFromToken();
                Username = string.Empty;
                Password = string.Empty;
                
                MessageBus.Current.SendMessage(
                    userInfo,
                    MessageTokens.GoToMain);
            }
            else
            {
                StatusMessage = $"{errorMessage}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка: {ex.Message}";
        }
    }
}
