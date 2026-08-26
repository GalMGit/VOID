using Avalonia.Controls;
using Avalonia.Threading;
using DialogHostAvalonia;

namespace VOID.APP.Views.Pages.Layout;

public partial class LayoutView : UserControl
{
    public LayoutView()
    {
        InitializeComponent();
    }

    private void Dialog_OnDialogClosing(object? sender, DialogClosingEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Dialog.DialogContent = null;
        });
    }
}