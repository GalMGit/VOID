using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using VOID.APP.ViewModels.Pages.Auth.Login;

namespace VOID.APP.Views.Pages.Auth.Login;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
    }

    private void Password_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox && DataContext is LoginViewModel vm)
            textBox.RevealPassword = vm.ShowPassword;
    }
}