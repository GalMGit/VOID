using System;
using System.Reactive.Disposables;

namespace VOID.APP.ViewModels.Pages.Auth.AuthLayout;

public partial class AuthLayoutViewModel : IDisposable
{
    private readonly CompositeDisposable _disposables = [];

    public void Dispose()
    {
        if (CurrentAuthPage is IDisposable profile)
            profile.Dispose();
        
        _disposables.Dispose();
        GC.SuppressFinalize(this);
    }
}