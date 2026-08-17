using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using VOID.APP.Models.Navigation;
using VOID.APP.Models.User;
using VOID.APP.ViewModels.Base.PageBase;

namespace VOID.APP.ViewModels.Pages.Layout.Search;

public partial class SearchListViewModel : PageViewModelBase
{
    
    private readonly ObservableCollection<SearchUserResponse> _searchResults;
    [Reactive] public partial SearchUserResponse SelectedUser { get; set; }
    public ObservableCollection<SearchUserResponse> SearchResults => _searchResults;
    
    public SearchListViewModel(ObservableCollection<SearchUserResponse> searchResults)
    {
        _searchResults = searchResults;

        this.WhenAnyValue(x => x.SelectedUser)
            .Subscribe(u =>
            {
                MessageBus.Current.SendMessage(
                    u, 
                    MessageTokens.SearchUser);
            }).DisposeWith(_disposables);
    }
}