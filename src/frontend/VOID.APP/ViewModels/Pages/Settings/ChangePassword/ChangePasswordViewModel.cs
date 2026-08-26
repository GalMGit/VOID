using System;
using System.Reactive;
using System.Threading.Tasks;
using MsBox.Avalonia;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using VOID.APP.Services.Interfaces.IAuth;
using VOID.APP.ViewModels.Base.ModalBase;
using VOID.APP.ViewModels.Base.PageBase;

namespace VOID.APP.ViewModels.Pages.Settings.ChangePassword;

public partial class ChangePasswordViewModel : PageViewModelBase
{
    private readonly IAuthService _authService;
    [Reactive] public partial string OldPassword { get; set; }
    [Reactive] public partial string NewPassword { get; set; }
    [Reactive] public partial bool IsBoxesHasValues { get; set; }

    public ReactiveCommand<Unit, Unit> GoToResetPasswordCommand { get; } = ReactiveCommand.Create(() =>
        MessageBus.Current.SendMessage(Unit.Default, "GoToResetPassword"));
    
    public ReactiveCommand<Unit, Unit> GoToMenuCommand { get; } = ReactiveCommand.Create(() =>
        MessageBus.Current.SendMessage(Unit.Default, "ToSettingsMenu"));
    

    public ChangePasswordViewModel(IAuthService authService)
    {
        _authService = authService;
        
        this.WhenAnyValue(x => x.OldPassword, x => x.NewPassword)
            .Subscribe(values =>
            {
                IsBoxesHasValues =
                    !string.IsNullOrWhiteSpace(values.Item1) &&
                    !string.IsNullOrWhiteSpace(values.Item2);
            });
    }

    [ReactiveCommand]
    private async Task ChangePasswordAsync()
    {
        var (success, errorMessage) = await _authService.ChangePasswordAsync(
            OldPassword, 
            NewPassword);

        if (success)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Успешно",
                "Пароль успешно изменен");

            await box.ShowAsync();
            MessageBus.Current.SendMessage(Unit.Default, "ToSettingsMenu");
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Ошибка",
                $"{errorMessage}");

            await box.ShowAsync();
        }

        return;
    }
    
}