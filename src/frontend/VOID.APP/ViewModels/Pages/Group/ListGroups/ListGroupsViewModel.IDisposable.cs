using System;
using System.Reactive.Disposables;

namespace VOID.APP.ViewModels.Pages.Group.ListGroups;

public partial class ListGroupsViewModel : IDisposable
{
    private readonly CompositeDisposable _disposables = [];
    
    public void Dispose()
    {
        foreach (var sub in _hubSubscriptions)
        {
            sub.Dispose();
        }
        _hubSubscriptions.Clear();
        Groups.Clear();
        _groupCache.Clear();
        _currentActiveGroupVm?.Deactivate();
        _disposables.Dispose();
        GC.SuppressFinalize(this);
    }
}