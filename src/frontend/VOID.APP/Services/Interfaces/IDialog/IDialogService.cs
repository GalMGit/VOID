using System.Threading.Tasks;
using VOID.APP.ViewModels.Base.ModalBase;

namespace VOID.APP.Services.Interfaces;

public interface IDialogService
{
    Task ShowAsync<TViewModel>(TViewModel viewModel) where TViewModel : ModalViewModelBase;
    void Close();
}