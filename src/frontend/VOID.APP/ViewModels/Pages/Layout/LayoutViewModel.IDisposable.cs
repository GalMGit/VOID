using System;
using System.Reactive.Disposables;

namespace VOID.APP.ViewModels.Pages.Layout;

public partial class LayoutViewModel : IDisposable
{
    private readonly CompositeDisposable _disposables = [];
    
    public void Dispose()
    {
        _disposables.Dispose();
        if (ProfileContent is IDisposable profile)
        {
            profile.Dispose();
            ProfileContent = null;
        }
        
        if (CurrentListContent is IDisposable currentList)
        {
            currentList.Dispose();
            CurrentListContent = null;
        }

        if (CurrentChatContent is IDisposable currentChat)
        {
            currentChat.Dispose();
            CurrentChatContent = null;
        }
        
        GC.SuppressFinalize(this);
    }
}