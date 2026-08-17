using System;
using System.Reactive.Disposables;

namespace VOID.APP.ViewModels.Pages.Auth.Register;

public partial class RegisterViewModel : IDisposable
{
    private readonly CompositeDisposable _disposables = [];
    public void Dispose()
    {
        StatusMessage = string.Empty;
        Username = string.Empty;
        Password = string.Empty;
        ConfirmPassword = string.Empty;
        Email = string.Empty;
        _disposables.Dispose();
        GC.SuppressFinalize(this);
    }
}