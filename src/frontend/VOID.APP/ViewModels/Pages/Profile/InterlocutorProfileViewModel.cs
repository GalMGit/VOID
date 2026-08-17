using System.Reactive;
using DialogHostAvalonia;
using ReactiveUI;
using VOID.APP.Models.Chat;
using VOID.APP.ViewModels.Base.ModalBase;

namespace VOID.APP.ViewModels.Pages.Profile;

public class InterlocutorProfileViewModel : ModalViewModelBase
{
    public FullChatModel Chat { get; set; }
    
    public InterlocutorProfileViewModel(FullChatModel chat)
        => Chat = chat;
}