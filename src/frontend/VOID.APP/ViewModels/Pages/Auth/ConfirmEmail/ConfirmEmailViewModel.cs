using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using VOID.APP.Models.Navigation;
using VOID.APP.Services.Interfaces.IAuth;
using VOID.APP.ViewModels.Base.PageBase;

namespace VOID.APP.ViewModels.Pages.Auth.ConfirmEmail;

public partial class ConfirmEmailViewModel : PageViewModelBase
{
    private readonly IAuthService _authService;
    
    [Reactive] public partial string ConfirmationCode { get; set; }
    [Reactive] public partial string Email { get; set; }
    [Reactive] public partial string StatusMessage { get; set; }
    
    public ReactiveCommand<Unit, Unit> GoToRegisterCommand { get; }

    public ConfirmEmailViewModel(IAuthService authService)
    {
        _authService = authService;
        
        MessageBus.Current.Listen<string>(MessageTokens.EmailSend)
            .Subscribe(x => Email = x);
        
        GoToRegisterCommand = ReactiveCommand.Create(() =>
        {
            ConfirmationCode = string.Empty;
            Email = string.Empty;
            StatusMessage = string.Empty;
            
            MessageBus.Current.SendMessage(
                Unit.Default,
                MessageTokens.GoToRegister);
            
        }).DisposeWith(_disposables);
    }

    [ReactiveCommand]
    private async Task SendConfirmationCode()
    {
        if (string.IsNullOrWhiteSpace(ConfirmationCode))
            return;

        var (success, errorMessage) = await _authService.ConfirmEmailAsync(
            ConfirmationCode, 
            Email);

        if (success)
        {
            ConfirmationCode = string.Empty;
            Email = string.Empty;
            
            MessageBus.Current.SendMessage(
                Unit.Default,
                MessageTokens.GoToLogin);
        }
        else
        {
            StatusMessage = $"{errorMessage}";
        }
    }
}
