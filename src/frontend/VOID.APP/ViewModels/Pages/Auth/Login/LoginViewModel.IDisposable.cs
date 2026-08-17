using System;
using System.Reactive.Disposables;
using ReactiveUI.SourceGenerators;

namespace VOID.APP.ViewModels.Pages.Auth.Login;

public partial class LoginViewModel : IDisposable
{
    private readonly CompositeDisposable _disposables = [];
    
    public void Dispose()
    {
        Username = string.Empty;
        Password = string.Empty;
        StatusMessage = string.Empty;
        _disposables.Dispose();
        GC.SuppressFinalize(this);
    }
}