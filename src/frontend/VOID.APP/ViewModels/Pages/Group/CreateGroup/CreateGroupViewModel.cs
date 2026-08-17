using System;
using System.Reactive;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using MsBox.Avalonia;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using VOID.APP.Models.Navigation;
using VOID.APP.Services.Interfaces.IGroup;
using VOID.APP.ViewModels.Base.ModalBase;

namespace VOID.APP.ViewModels.Pages.Group.CreateGroup;

public partial class CreateGroupViewModel : ModalViewModelBase
{
    private readonly IGroupService _groupService;
    [Reactive] public partial string GroupName { get; set; }
    private int _groupsCount;

    public CreateGroupViewModel(IGroupService groupService)
    {
        _groupService = groupService;
         MessageBus.Current.Listen<int>(MessageTokens.SendGroupCount)
            .Subscribe(x => _groupsCount = x);
    }

    [ReactiveCommand]
    private async Task CreateGroupAsync()
    {
        if (string.IsNullOrWhiteSpace(GroupName))
            return;
        
        MessageBus.Current.SendMessage(
            Unit.Default, 
            MessageTokens.CheckGroupCount);
        
        if (_groupsCount >= 3)
        {
            GroupName = string.Empty;
            MessageBus.Current.SendMessage(
                Unit.Default,
                MessageTokens.CloseDialog);
            
            var box = MessageBoxManager.GetMessageBoxStandard(
                        "Ошибка", "Максимум групп - 3");

            await box.ShowAsync();
            return;
        }

        await _groupService.CreateGroupAsync(GroupName);

        GroupName = string.Empty;
        MessageBus.Current.SendMessage(
            Unit.Default, 
            MessageTokens.GroupCreated);
    }
}