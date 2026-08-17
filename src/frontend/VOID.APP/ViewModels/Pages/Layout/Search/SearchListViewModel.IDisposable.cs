using System;
using System.Reactive.Disposables;

namespace VOID.APP.ViewModels.Pages.Layout.Search;

public partial class SearchListViewModel : IDisposable
{
    private readonly CompositeDisposable _disposables = [];
    
    public void Dispose()
        => _disposables.Dispose();
    
}