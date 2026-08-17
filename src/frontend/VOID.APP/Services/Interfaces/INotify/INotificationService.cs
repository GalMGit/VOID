using System.Threading.Tasks;

namespace VOID.APP.Services.Interfaces.INotify;

public interface INotificationService
{
    Task ShowNotificationAsync(string title, string message);
}