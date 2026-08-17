using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using VOID.APP.Extensions;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Tmds.DBus.Protocol;
using VOID.APP.Models.Group;
using VOID.APP.Models.Navigation;
using VOID.APP.Models.User;
using VOID.APP.Services.Interfaces.IGroup;
using VOID.APP.ViewModels.Base.ModalBase;

namespace VOID.APP.ViewModels.Pages.Group.AddMember;

public partial class AddMemberViewModel : ModalViewModelBase
{
    private readonly IGroupService _groupService;
    private readonly GroupModel _groupModel;
    private readonly HubConnection _hubConnection;

    public ObservableCollection<SearchUserResponse> SearchUsers { get; set; } = [];
    public ObservableCollection<SearchUserResponse> UsersForAdd { get; set; } = [];

    [Reactive] public partial string SearchText { get; set; }

    public AddMemberViewModel(
        GroupModel groupModel,
        IGroupService groupService,
        HubConnection hubConnection)
    {
        _groupService = groupService;
        _groupModel = groupModel;
        _hubConnection = hubConnection;
    }

    [ReactiveCommand]
    private async Task SearchUsersAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
            return;

        var searchUsers = await _groupService.SearchUsersForGroupAsync(
            _groupModel.Id,
            SearchText);

        await SearchUsers.UpdateFromAsync(searchUsers ?? []);
    }

    [ReactiveCommand]
    private void AddUserToMembersForAdd(SearchUserResponse user)
    {
        UsersForAdd.Add(user);
        SearchText = string.Empty;
        SearchUsers.Clear();
    }

    [ReactiveCommand]
    private void RemoveUserFromMembersForAdd(SearchUserResponse user)
        => UsersForAdd.Remove(user);
    

    [ReactiveCommand]
    private async Task AddMembersAsync()
    {
        if (!UsersForAdd.Any())
            return;

        var memberIds = UsersForAdd
            .Select(x => x.Id)
            .ToList();
        
        var addedMembers = await _groupService.AddMembersAsync(
            memberIds, 
            _groupModel.Id);
        
        SearchUsers.Clear();
        UsersForAdd.Clear();
        
        MessageBus.Current.SendMessage(
            Unit.Default, 
            MessageTokens.CloseDialog);
        
        MessageBus.Current.SendMessage(
            addedMembers, 
            MessageTokens.MembersAdded);

        await _hubConnection.SendAsync(
            SignalREvents.AddToGroupEvent,
            _groupModel,
            memberIds);
    }
}