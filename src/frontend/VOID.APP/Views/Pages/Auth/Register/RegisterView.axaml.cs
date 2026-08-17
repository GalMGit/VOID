using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using VOID.APP.ViewModels.Pages.Auth.Register;

namespace VOID.APP.Views.Pages.Auth.Register;

public partial class RegisterView : UserControl
{
    public RegisterView()
    {
        InitializeComponent();
    }

    private void Password_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox && DataContext is RegisterViewModel vm)
            textBox.RevealPassword = vm.ShowPassword;
    }

    private void ConfirmPassword_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox && DataContext is RegisterViewModel vm)
            textBox.RevealPassword = vm.ShowConfirmPassword;
    }
}