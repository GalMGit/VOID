using System;
using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using ReactiveUI;
using VOID.APP.Models.Navigation;

namespace VOID.APP.Views.Pages.Chat.ListChats;

public partial class ListChatsView : UserControl
{
    private ScrollViewer? _scrollViewer;
    private double _lastScrollOffset;

    public ListChatsView()
    {
        InitializeComponent();
        ListBox.AddHandler(
            InputElement.PointerPressedEvent,
            ListBox_OnPointerPressed,
            RoutingStrategies.Tunnel);
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        _scrollViewer = this.FindControl<ScrollViewer>("ChatsScrollViewer");

        if (_scrollViewer is not null)
        {
            _scrollViewer.PointerWheelChanged += OnPointerWheelChanged;
        }
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (_scrollViewer is null) return;

        var currentOffset = _scrollViewer.Offset.Y;
        var maxOffset = _scrollViewer.Extent.Height - _scrollViewer.Viewport.Height;

        var isAtBottom = currentOffset >= maxOffset - 50;

        bool isScrollingDown = e.Delta.Y < 0;

        if (isScrollingDown && isAtBottom)
        {
            MessageBus.Current.SendMessage(Unit.Default, MessageTokens.ChatsLoadNextPage);
        }

        _lastScrollOffset = currentOffset;
    }
    
    private void ListBox_OnPointerPressed(
        object? sender,
        PointerPressedEventArgs e)
    {
        if (e.Source is not Control source)
            return;

        var item = source.FindAncestorOfType<ListBoxItem>();

        if (item is null)
            return;

        var point = e.GetCurrentPoint(item);

        // Правый клик — не меняем selection
        if (point.Properties.IsRightButtonPressed)
        {
            e.Handled = true;
            return;
        }
        
        if (!point.Properties.IsLeftButtonPressed)
            return;

        if (item.IsSelected)
        {
            item.IsSelected = false;
            e.Handled = true;
        }
    }
}