using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using VOID.APP.Models.Navigation;
using VOID.APP.ViewModels.Pages.Layout;

namespace VOID.APP.ViewModels.Window;

public partial class MainWindowViewModel
{
    private async Task StartHubConnectionAsync()
    {
        if (_hubConnection.State == HubConnectionState.Disconnected)
            await _hubConnection.StartAsync();
    }

    private async Task HandleLogout()
    {
        await BeforeLogout();
        await _authService.Logout();
        CurrentPage = _viewModelFactory.CreateAuthLayout();
    }

    private async Task BeforeLogout()
    {
        await _hubConnection.StopAsync();

        if(CurrentPage is LayoutViewModel layout)
            layout.Dispose();
    }

    public async Task HandleExit()
    {
        await BeforeLogout();
        await _hubConnection.DisposeAsync();
    }
}
