using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Styling;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using VOID.APP.Models.Navigation;
using VOID.APP.Services.Interfaces;
using VOID.APP.Services.Interfaces.ISettings;
using VOID.APP.ViewModels.Base.PageBase;
using VOID.APP.ViewModels.Pages.Settings.ChangePassword;

namespace VOID.APP.ViewModels.Pages.Auth.AuthLayout;

public partial class AuthLayoutViewModel : PageViewModelBase
{
    private readonly IViewModelFactory _viewModelFactory;
    private readonly ISettingsService _settingsService;
    [Reactive] public partial PageViewModelBase? CurrentAuthPage { get; set; }
    [Reactive] public partial bool IsPageReset { get; set; }
    private List<PageViewModelBase> ListPages { get; set; }
    [Reactive] public partial bool IsThemeChecked { get; set; }
    
    

    public AuthLayoutViewModel(
        IViewModelFactory viewModelFactory,
        ISettingsService settingsService
    )
    {
        _viewModelFactory = viewModelFactory;
        _settingsService = settingsService;
        
        ListPages = [
            viewModelFactory.CreateLogin(),
            viewModelFactory.CreateRegister(),
            viewModelFactory.CreateConfirmEmail(),
            viewModelFactory.CreateResetPassword()

        ];
        _ = LoadThemeAsync();
        CurrentAuthPage = ListPages[0];

        this.WhenAnyValue(x => x.CurrentAuthPage)
            .Subscribe(page =>
            {
                IsPageReset = page is ResetPasswordViewModel;
            });

        SetupNavigation();
    }

    [ReactiveCommand]
    private async Task GoToMain()
    {
        if (CurrentAuthPage is ResetPasswordViewModel)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Предупреждение",
                "Если вы покинете эту страницу, то можете потерять прогресс восстановления пароля",
                ButtonEnum.OkCancel);

            var result = await box.ShowAsync();

            if (result == ButtonResult.Cancel)
                return;

        }
        CurrentAuthPage = _viewModelFactory.CreateLogin();
    }
    
    private async Task LoadThemeAsync()
    {
        var settings = await _settingsService.LoadSettingsAsync();

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            IsThemeChecked = settings.Theme == "Dark";
        });
    }
    
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
    
    private void SetupNavigation()
    {
        MessageBus.Current.Listen<Unit>(MessageTokens.GoToLogin)
            .Subscribe(_ => CurrentAuthPage = ListPages[0])
            .DisposeWith(_disposables);

        MessageBus.Current.Listen<Unit>(MessageTokens.GoToRegister)
            .Subscribe(_ => CurrentAuthPage = ListPages[1])
            .DisposeWith(_disposables);

        MessageBus.Current.Listen<Unit>(MessageTokens.GoToConfirm)
            .Subscribe(_ => CurrentAuthPage = ListPages[2])
            .DisposeWith(_disposables);
        
        MessageBus.Current.Listen<Unit>(MessageTokens.GoToResetPass)
            .Subscribe(_ => CurrentAuthPage = ListPages[3])
            .DisposeWith(_disposables);
    }
}
