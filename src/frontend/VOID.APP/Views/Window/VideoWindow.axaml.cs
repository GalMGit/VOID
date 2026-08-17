using System;
using Avalonia.Controls;
using VOID.APP.ViewModels.Modals.Video;

namespace VOID.APP.Views.Window;

public partial class VideoWindow : Avalonia.Controls.Window
{
    public VideoWindow()
    {
        InitializeComponent();
    }
    
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        PlayerView?.MediaPlayerViewModel?.MediaPlayer?.Stop();
        base.OnClosing(e);
    }
}