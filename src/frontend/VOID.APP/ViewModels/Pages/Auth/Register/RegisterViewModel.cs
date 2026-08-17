using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using VOID.APP.Models.Navigation;
using VOID.APP.Services.Interfaces.IAuth;
using VOID.APP.ViewModels.Base.PageBase;

namespace VOID.APP.ViewModels.Pages.Auth.Register;

public partial class RegisterViewModel : PageViewModelBase
{
    private readonly IAuthService _authService;

    [Reactive] public partial string Email { get; set; }
    [Reactive] public partial string Username { get; set; }
    [Reactive] public partial string Password { get; set; }
    [Reactive] public partial string ConfirmPassword { get; set; }
    [Reactive] public partial string StatusMessage { get; set; }
    [Reactive] public partial bool ShowPassword { get; set; }
    [Reactive] public partial bool ShowConfirmPassword { get; set; }

    public ReactiveCommand<Unit, Unit> GoToLoginCommand { get; }

    public RegisterViewModel(IAuthService authService)
    {
        _authService = authService;

        GoToLoginCommand = ReactiveCommand.Create(() =>
        {
            Username = string.Empty;
            Email = string.Empty;
            ConfirmPassword = string.Empty;
            Password = string.Empty;
            StatusMessage = string.Empty;
            ShowPassword = false;
            ShowConfirmPassword = false;
            
            MessageBus.Current.SendMessage(
                Unit.Default,
                MessageTokens.GoToLogin);
            
        }).DisposeWith(_disposables);
    }

    [ReactiveCommand]
    private async Task RegisterAsync()
    {
        try
        {
            var (success, errorMessage) = await _authService.RegisterAsync(
                    Email,
                    Username,
                    Password,
                    ConfirmPassword);

            if (success)
            {
                Username = string.Empty;
                Password = string.Empty;
                ConfirmPassword = string.Empty;
                StatusMessage = string.Empty;
                
                MessageBus.Current.SendMessage(Email,
                    MessageTokens.EmailSend);
                
                MessageBus.Current.SendMessage(
                    Unit.Default, 
                    MessageTokens.GoToConfirm);
                
                Email = string.Empty;
            }
            else
            {
                StatusMessage = $"{errorMessage}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"{ex.Message}";
        }
    }
}
