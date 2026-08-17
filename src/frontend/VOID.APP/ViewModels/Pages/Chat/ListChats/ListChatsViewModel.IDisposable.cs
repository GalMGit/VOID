using System;
using System.Reactive.Disposables;

namespace VOID.APP.ViewModels.Pages.Chat.ListChats;

public partial class ListChatsViewModel : IDisposable
{
    private readonly CompositeDisposable _disposables = [];

    public void Dispose()
    {
        foreach (var sub in _hubSubscriptions)
        {
            sub.Dispose();
        }
        _hubSubscriptions.Clear();
        _chatCache.Clear();
        Chats.Clear();
        _currentPage = 1;
        _totalPages = 0;
        _hasNextPages = false;
        _currentActiveChatVm?.Dispose();
        _currentActiveChatVm?.Deactivate();
        _disposables.Dispose();
        GC.SuppressFinalize(this);
    }
}