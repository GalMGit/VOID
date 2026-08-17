using System;
using System.Threading.Tasks;
using Avalonia.Labs.Notifications;
using VOID.APP.Services.Interfaces.INotify;

namespace VOID.APP.Services.Implementations.INotify;

public sealed class NotificationService : INotificationService
{
    public Task ShowNotificationAsync(
        string title,
        string message)
    {
        var manager = NativeNotificationManager.Current;

        var notification = manager?.CreateNotification(null);

        if (notification is null)
            return Task.CompletedTask;

        notification.Title = title;
        notification.Message = message;
        notification.Expiration = TimeSpan.FromSeconds(5);
        
        notification.Show();

        return Task.CompletedTask;
    }
}