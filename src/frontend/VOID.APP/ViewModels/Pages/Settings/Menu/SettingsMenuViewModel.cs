using System.Reactive;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using VOID.APP.Services.Interfaces;
using VOID.APP.ViewModels.Base.ModalBase;
using VOID.APP.ViewModels.Base.PageBase;

namespace VOID.APP.ViewModels.Pages.Settings.Menu;

public partial class SettingsMenuViewModel : PageViewModelBase
{
    public ReactiveCommand<Unit, Unit> OpenChangePasswordDialogCommand { get; set; }
    
    public SettingsMenuViewModel()
    {
        OpenChangePasswordDialogCommand = ReactiveCommand.Create(() =>
            MessageBus.Current.SendMessage(Unit.Default, "OpenChangePassword"));
    }
}