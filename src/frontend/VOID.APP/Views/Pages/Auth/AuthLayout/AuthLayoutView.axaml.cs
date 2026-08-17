using Avalonia.Controls;
using System.Threading.Tasks;
using Avalonia.Threading;
using System;

namespace VOID.APP.Views.Pages.Auth.AuthLayout;

public partial class AuthLayoutView : UserControl
{
    private readonly string _fullText = "VOID";
    public AuthLayoutView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }
    private async void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await TypeText();
        await BlinkCursorFiveTimes();
    }

    private async Task TypeText()
    {
        for (var i = 0; i <= _fullText.Length; i++)
        {
            TitleText.Text = _fullText[..i];
            await Task.Delay(90); 
        }
    }

    private async Task BlinkCursorFiveTimes()
    {
        for (var blink = 0; blink < 5; blink++)
        {
            Cursor.Opacity = 1;
            await Task.Delay(300);
            
            Cursor.Opacity = 0;
            await Task.Delay(300);
        }
        
        Cursor.Opacity = 0;
    }
}