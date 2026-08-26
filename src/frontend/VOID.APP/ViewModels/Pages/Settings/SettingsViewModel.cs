using System;
using System.Reactive;
using System.Threading.Tasks;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Tmds.DBus.Protocol;
using VOID.APP.Services.Interfaces;
using VOID.APP.ViewModels.Base.ModalBase;
using VOID.APP.ViewModels.Base.PageBase;
using VOID.APP.ViewModels.Pages.Settings.ChangePassword;
using VOID.APP.ViewModels.Pages.Settings.Menu;

namespace VOID.APP.ViewModels.Pages.Settings;

public partial class SettingsViewModel : ModalViewModelBase
{
    private readonly IViewModelFactory _viewModelFactory;
    [Reactive] public partial PageViewModelBase? CurrentSettingsContent { get; set; }
    [Reactive] public partial bool IsHomeButtonVisible { get; set; }
    
    public SettingsViewModel(
        IViewModelFactory viewModelFactory)
    {
        _viewModelFactory = viewModelFactory;

        CurrentSettingsContent ??= _viewModelFactory.CreateSettingsMenu();

        this.WhenAnyValue(x => x.CurrentSettingsContent)
            .Subscribe(page =>
            {
                IsHomeButtonVisible = page is not SettingsMenuViewModel;
            });
        
        SetupSubscriptions();
    }

    [ReactiveCommand]
    private async Task GoToSettingsMenu()
    {
        if (CurrentSettingsContent is ResetPasswordViewModel)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Предупреждение",
                "Если вы покинете эту страницу, то можете потерять прогресс восстановления пароля",
                ButtonEnum.OkCancel);

            var result = await box.ShowAsync();

            if (result == ButtonResult.Cancel)
                return;
        }
        
        CurrentSettingsContent = _viewModelFactory.CreateSettingsMenu();
    } 
    
    private void SetupSubscriptions()
    {
        MessageBus.Current.Listen<Unit>("OpenChangePassword")
            .Subscribe(_ =>
            {
                CurrentSettingsContent = _viewModelFactory.CreateChangePassword();
            });

        MessageBus.Current.Listen<Unit>("GoToResetPassword")
            .Subscribe(_ =>
            {
                CurrentSettingsContent = _viewModelFactory.CreateResetPassword();
            });
        
    }
}