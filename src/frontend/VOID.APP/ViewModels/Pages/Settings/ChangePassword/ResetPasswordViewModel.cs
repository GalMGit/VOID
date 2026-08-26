using System.Reactive;
using System.Threading.Tasks;
using MsBox.Avalonia;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using VOID.APP.Services.Interfaces.IAuth;
using VOID.APP.ViewModels.Base.PageBase;

namespace VOID.APP.ViewModels.Pages.Settings.ChangePassword;

public partial class ResetPasswordViewModel : PageViewModelBase
{
    private readonly IAuthService _authService;
    
    [Reactive] public partial string Email { get; set; }
    [Reactive] public partial string Code { get; set; }
    [Reactive] public partial string Token { get; set; }
    [Reactive] public partial string NewPassword { get; set; }
    [Reactive] public partial bool IsEmailSend { get; set; }
    [Reactive] public partial bool IsCodeSend { get; set; }
    
    
    public ReactiveCommand<Unit, Unit> GoToMenuCommand { get; } = ReactiveCommand.Create(() =>
        MessageBus.Current.SendMessage(Unit.Default, "ToSettingsMenu"));
    
    public ResetPasswordViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    [ReactiveCommand]
    private async Task SendResetPassword()
    {
        if(string.IsNullOrWhiteSpace(Email))
            return;
        
        await _authService.SendResetPasswordAsync(
            Email);
        
        IsEmailSend = true;
    }

    [ReactiveCommand]
    private async Task CreateNewPassword()
    {
        if (string.IsNullOrWhiteSpace(Token) || string.IsNullOrWhiteSpace(NewPassword))
            return;
        
        
        var (success, error) = await _authService.CompleteResetPasswordAsync(
            Token, NewPassword);
        
        if (success)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Успешно",
                "Пароль успешно восстановлен");

            await box.ShowAsync();
            
            MessageBus.Current.SendMessage(Unit.Default, "ToSettingsMenu");
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Ошибка",
                $"{error}");

            await box.ShowAsync();
        }
    }
    
    [ReactiveCommand]
    private async Task SendCode()
    {
        if (string.IsNullOrWhiteSpace(Code))
            return;
        
        var (success, error, token) = await _authService.SendResetCodeAsync(
            Email, 
            Code);

        if (success)
        {
            IsCodeSend = true;
            Token = token;
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Ошибка",
                $"{error}");

            await box.ShowAsync();
        }
    }
}