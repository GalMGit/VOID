using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using AsyncImageLoader;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Iciclecreek.Avalonia.Controls.Media;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using VOID.APP.Models.Messages;
using VOID.APP.Models.Navigation;
using VOID.APP.ViewModels.Modals.Image;
using VOID.APP.ViewModels.Modals.Video;
using VOID.APP.ViewModels.Pages.Chat.CurrentChat;
using VOID.APP.Views.Window;

namespace VOID.APP.Views.Pages.Chat.CurrentChat;

public partial class CurrentChatView : ReactiveUserControl<CurrentChatView>
{
    private ScrollViewer? _scrollViewer;
    private const double ScrollThreshold = 40.0;

    public CurrentChatView()
    {
        InitializeComponent();
        
        MessageListBox.AddHandler(
            InputElement.PointerPressedEvent,
            MessageListBox_OnPointerPressed,
            RoutingStrategies.Tunnel);

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        
        this.WhenActivated(disposables =>
        {
            disposables.Add(MessageBus.Current.Listen<Unit>(MessageTokens.ScrollToBottom)
                .Subscribe(_ => ScrollToBottom())
                .DisposeWith(disposables));

            disposables.Add(MessageBus.Current.Listen<ScrollInfoRequest>(MessageTokens.GetScrollPosition)
                .Subscribe(HandleScrollInfoRequest)
                .DisposeWith(disposables));

             // Dispatcher.UIThread.Post(ScrollToBottom);
        });
        
        var isAtBottom = _scrollViewer?.Offset.Y >= _scrollViewer?.Extent.Height - _scrollViewer?.Viewport.Height - 20;
    }

    private void OnLoaded(
        object? sender, 
        RoutedEventArgs e)
    {
        _scrollViewer = this.FindControl<ScrollViewer>("MessageScrollViewer");
        if (_scrollViewer is not null)
        {
            _scrollViewer.PointerWheelChanged += OnPointerWheelChanged;
            _scrollViewer.ScrollChanged += OnScrollChanged;
        }
    }

    private void OnUnloaded(
        object? sender, 
        RoutedEventArgs e)
    {
        if (_scrollViewer is not null)
        {
            _scrollViewer.PointerWheelChanged -= OnPointerWheelChanged;
            _scrollViewer.ScrollChanged -= OnScrollChanged;
        }
    }

    private void OnScrollChanged(
        object? sender, 
        RoutedEventArgs e)
    {
        if (_scrollViewer is null) return;

        var isAtBottom = _scrollViewer.Offset.Y >= _scrollViewer.Extent.Height - _scrollViewer.Viewport.Height - 20;

        if (isAtBottom)
        {
            MessageBus.Current.SendMessage(
                Unit.Default, 
                MessageTokens.SendIsReadMessages);
        }
    }

    private void OnPointerWheelChanged(
        object? sender,
        PointerWheelEventArgs e)
    {
        if (_scrollViewer is null)
            return;

        bool isScrollingUp = e.Delta.Y > 0;

        bool isAtTop = _scrollViewer.Offset.Y <= 10;

        if (isScrollingUp && isAtTop)
        {
            MessageBus.Current.SendMessage(
                Unit.Default,
                MessageTokens.MessagesLoadNextPage);
        }
    }

    private void ScrollToBottom()
        => _scrollViewer?.ScrollToEnd();
    

    private void HandleScrollInfoRequest(ScrollInfoRequest request)
    {
        var scrollViewer = MessageScrollViewer;
        var extentHeight = scrollViewer.Extent.Height;
        var viewportHeight = scrollViewer.Viewport.Height;
        var offset = scrollViewer.Offset.Y;

        request.IsNearBottom = (extentHeight - (offset + viewportHeight)) < ScrollThreshold;
    }

    private async void InputElement_OnPointerPressed(
        object? sender, 
        PointerPressedEventArgs e)
    {
        if (sender is Image { DataContext: MessageModel message })
        {
            var url = message.ImageUrl;
            var window = new ImageWindow
            {
                DataContext = new ImageWindowViewModel(url)
            };
            
            var currentWindow = Avalonia.Application.Current?.ApplicationLifetime 
                as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;
            
            var mainWindow = currentWindow?.MainWindow;
        
            if (mainWindow != null)
            {
                await window.ShowDialog(mainWindow);
            }
            else
            {
                window.Show();
            }
        }
    }

    private async void InputVideoElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border { DataContext: MessageModel message })
        {
            var factory = App.ServiceProvider!.GetRequiredService<Func<Guid, VideoWindowViewModel>>();
            var window = new VideoWindow
            {
                DataContext = factory(message.Id)
            };
            
            var currentWindow = Avalonia.Application.Current?.ApplicationLifetime 
                as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;
            var mainWindow = currentWindow?.MainWindow;
        
            if (mainWindow != null)
            {
                await window.ShowDialog(mainWindow);
            }
            else
            {
                window.Show();
            }
        }
    }

    private void MessageListBox_OnPointerPressed(
        object? sender,
        PointerPressedEventArgs e)
    {
        if (e.Source is not Control source)
            return;

        var item = source.FindAncestorOfType<ListBoxItem>();

        if (item is null)
            return;

        if (item.DataContext is not MessageModel message)
            return;

        var point = e.GetCurrentPoint(item);

        // Правый клик — не меняем selection
        if (point.Properties.IsRightButtonPressed)
        {
            e.Handled = true;
            return;
        }
        
        if (!message.IsMine)
        {
            e.Handled = true;
            return;
        }

        // Левая кнопка
        if (!point.Properties.IsLeftButtonPressed)
            return;
        

        if (item.IsSelected)
        {
            item.IsSelected = false;
            e.Handled = true;
        }
    }
}