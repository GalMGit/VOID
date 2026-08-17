using Avalonia;
using Avalonia.ReactiveUI;
using System;
using Avalonia.Labs.Notifications;
using System.Diagnostics;

namespace VOID.APP;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .WithAppNotifications(new AppNotificationOptions
            {
                AppUserModelId = Process.GetCurrentProcess().ProcessName,
                AppName = "VOID"
            })
            .UseReactiveUI();
}