using System;
using System.Reactive.Disposables;

namespace VOID.APP.ViewModels.Pages.Profile;

public partial class ProfileViewModel : IDisposable
{
    private readonly CompositeDisposable _disposables = [];
    public void Dispose()
    {
        _disposables.Dispose();
        UserModel.AvatarUrl = string.Empty;
        GC.SuppressFinalize(this);
    }
}