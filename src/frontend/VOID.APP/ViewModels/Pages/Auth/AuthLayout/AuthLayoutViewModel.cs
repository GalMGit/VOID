using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Styling;
using VOID.APP.ViewModels.Pages.Auth.Login;
using VOID.APP.ViewModels.Pages.Auth.Register;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using VOID.APP.Models.Navigation;
using VOID.APP.Services.Interfaces;
using VOID.APP.ViewModels.Base.PageBase;

namespace VOID.APP.ViewModels.Pages.Auth.AuthLayout;

public partial class AuthLayoutViewModel : PageViewModelBase
{
    [Reactive] public partial PageViewModelBase? CurrentAuthPage { get; set; }
    private List<PageViewModelBase> ListPages { get; set; }

    public AuthLayoutViewModel(
        IViewModelFactory viewModelFactory
    )
    {
        ListPages = [
            viewModelFactory.CreateLogin(),
            viewModelFactory.CreateRegister(),
            viewModelFactory.CreateConfirmEmail()

        ];
        CurrentAuthPage = ListPages[0];

        SetupNavigation();
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
    }
}
