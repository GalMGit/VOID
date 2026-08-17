using System.Reactive;
using ReactiveUI;
using VOID.APP.Models.Navigation;
using VOID.APP.Services.Interfaces.IAuth;

namespace VOID.APP.Services.Implementations.Auth;

public class AuthErrorHandler : IAuthErrorHandler
{
    private bool _isRedirecting;

    public void HandleUnauthorized()
    {
        if (_isRedirecting)
            return;

        _isRedirecting = true;

        MessageBus.Current.SendMessage(
            Unit.Default, 
            MessageTokens.ErrorAuthLogout);

        _isRedirecting = false;
    }
}
