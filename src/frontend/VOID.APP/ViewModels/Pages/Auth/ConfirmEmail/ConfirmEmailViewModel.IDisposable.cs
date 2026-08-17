using System;
using System.Reactive.Disposables;

namespace VOID.APP.ViewModels.Pages.Auth.ConfirmEmail;

public partial class ConfirmEmailViewModel : IDisposable
{
    private readonly CompositeDisposable _disposables = [];

    public void Dispose()
    {
        ConfirmationCode = string.Empty;
        Email = string.Empty;
        StatusMessage = string.Empty;
        _disposables.Dispose();
        GC.SuppressFinalize(this);
    }
}
