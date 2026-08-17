
using System.Threading.Tasks;
using DialogHostAvalonia;
using VOID.APP.Models.Navigation;
using VOID.APP.Services.Interfaces;
using VOID.APP.ViewModels.Base.ModalBase;

namespace VOID.APP.Services.Implementations;

public class DialogService : IDialogService
{
    public async Task ShowAsync<TViewModel>(TViewModel viewModel) where TViewModel : ModalViewModelBase
        => await DialogHost.Show(viewModel, DialogNames.Dialog);
    

    public void Close()
        => DialogHost.Close(DialogNames.Dialog);
    
}