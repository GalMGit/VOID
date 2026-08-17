using System;
using System.IO;
using System.Net.Http;
using AsyncImageLoader;
using AsyncImageLoader.Loaders;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HotAvalonia;
using Microsoft.Extensions.DependencyInjection;
using VOID.APP.DI;
using VOID.APP.ViewModels.Window;
using VOID.APP.Views.Window;

namespace VOID.APP;

public partial class App : Application
{
    public static IServiceProvider? ServiceProvider { get; private set; }

    public override void Initialize()
    {
        this.EnableHotReload();
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var serviceCollection = new ServiceCollection();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            serviceCollection.AddSingleton(desktopLifetime);
        }

        DIConfig.ConfigureViewModels(serviceCollection);
        

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            ServiceProvider = serviceCollection.BuildServiceProvider();

            var httpClient = ServiceProvider.GetRequiredService<HttpClient>();
            
            ImageLoader.AsyncImageLoader = new DiskCachedWebImageLoader(
                httpClient,
                disposeHttpClient: false,
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Downloads",
                    "VOID",
                    "Cache",
                    "Images"));

            desktop.MainWindow = new MainWindow
            {
                DataContext = ServiceProvider.GetRequiredService<MainWindowViewModel>()
            };

            desktop.Exit += async (_, _) =>
            {
                if (desktop.MainWindow.DataContext is MainWindowViewModel vm)
                {
                    await vm.HandleExit();
                    vm.Dispose();
                }
            };
        }
        else
        {
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        base.OnFrameworkInitializationCompleted();
    }
}