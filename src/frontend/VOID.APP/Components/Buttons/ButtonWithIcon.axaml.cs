using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace VOID.APP.Components.Buttons;

public partial class ButtonWithIcon : UserControl
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<ButtonWithIcon, string>(nameof(Text));

    public  static readonly StyledProperty<ICommand> CommandProperty =
        AvaloniaProperty.Register<ButtonWithIcon, ICommand>(nameof(Command));

    public  static readonly StyledProperty<object> CommandParameterProperty =
        AvaloniaProperty.Register<ButtonWithIcon, object>(nameof(CommandParameter));
    
    public  static readonly StyledProperty<Geometry> MyIconDataProperty =
        AvaloniaProperty.Register<ButtonWithIcon, Geometry>(nameof(MyIconData));
    
    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    
    public ICommand Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public Geometry MyIconData
    {
        get => GetValue(MyIconDataProperty);
        set => SetValue(MyIconDataProperty, value);
    }
    
    public ButtonWithIcon()
    {
        InitializeComponent();
    }
}