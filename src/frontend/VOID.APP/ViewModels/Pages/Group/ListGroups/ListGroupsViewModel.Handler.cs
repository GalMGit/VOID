using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using Microsoft.AspNetCore.SignalR.Client;
using VOID.Shared.Contracts.SignalRTokens;
using ReactiveUI;
using VOID.APP.Models.Group;
using VOID.APP.Models.Navigation;

namespace VOID.APP.ViewModels.Pages.Group.ListGroups;

public partial class ListGroupsViewModel
{
    private void SetupSignalRHandlers()
    {
        _hubSubscriptions.Add(_hubConnection.On<GroupModel>(SignalRTokens.GroupCreated, HandleGroupCreated));
        _hubSubscriptions.Add(_hubConnection.On<GroupModel, Guid>(SignalRTokens.AddedToGroup, HandleToGroupAdded));
        _hubSubscriptions.Add(_hubConnection.On<Guid, Guid>(SignalRTokens.DeleteGroupMember, HandleDeleteGroupMember));
        _hubSubscriptions.Add(_hubConnection.On<Guid>(SignalRTokens.GroupDeleted, HandleGroupDeleted));
        _hubSubscriptions.Add(_hubConnection.On<Guid, string>(SignalRTokens.GroupImageUpdated, HandleGroupImageUpdated));
    }

    private async Task HandleGroupImageUpdated(
        Guid groupId, 
        string imageUrl)
    {
        var group = Groups
            .FirstOrDefault(x => x.Id == groupId);

        if (group is null) return;

        await Dispatcher.UIThread.InvokeAsync(() =>
            group.ImageUrl = imageUrl);
    }

    private async Task HandleGroupDeleted(Guid groupId)
    {
        var groupForDelete = Groups
            .FirstOrDefault(x => x.Id == groupId);
        
        if (groupForDelete is null) return;

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Groups.Remove(groupForDelete);
            
            MessageBus.Current.SendMessage(
                Unit.Default, 
                MessageTokens.ClearSelectionCurrent);
        });

        await _hubConnection.InvokeAsync(
            SignalREvents.RemoveFromGroupEvent, 
            groupId);
    }

    private async Task HandleDeleteGroupMember(
        Guid groupId, 
        Guid memberId)
    {
        var groupForDelete = Groups
            .FirstOrDefault(x => x.Id == groupId);
        
        if (groupForDelete is null) return;
        
        if (_currentUserId != memberId) return;

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Groups.Remove(groupForDelete);
            
            MessageBus.Current.SendMessage(
                Unit.Default, 
                MessageTokens.ClearSelectionCurrent);

        });
        
        await _hubConnection.InvokeAsync(
                SignalREvents.RemoveFromGroupEvent,
                groupId);
    }

    private async Task HandleToGroupAdded(
        GroupModel @event, 
        Guid senderId)
    {
        if (_currentUserId == senderId)
            return;

        await Dispatcher.UIThread.InvokeAsync(() =>
                Groups.Insert(0, @event));
        
        await _hubConnection.InvokeAsync(
            SignalREvents.JoinToGroupEvent,
            @event.Id);

        MessageBus.Current.SendMessage(
            true, 
            MessageTokens.GroupJoined);
    }

    private async Task HandleGroupCreated(GroupModel @event)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            @event.CurrentUserId = _currentUserId;
            
            Groups.Insert(
                0, 
                @event);
        });
    }

    private void SetupMessages()
    {
        MessageBus.Current.Listen<Unit>(MessageTokens.LoadGroups)
            .SelectMany(_ => Observable.FromAsync(async () => 
                await LoadGroups(1)))
            .Subscribe()
            .DisposeWith(_disposables);

        MessageBus.Current.Listen<Unit>(MessageTokens.ClearSelectionGroupList)
            .Subscribe(_ =>
            {
                _currentActiveGroupVm?.Deactivate();
                SelectedGroupItem = null;
            })
            .DisposeWith(_disposables);

        MessageBus.Current.Listen<Unit>(MessageTokens.CheckGroupCount)
            .Subscribe(_ =>
            {
                var groupsCount = Groups.Count;
                
                MessageBus.Current.SendMessage(
                    groupsCount, 
                    MessageTokens.SendGroupCount);
            })
            .DisposeWith(_disposables);

        this.WhenAnyValue(x => x.SelectedGroupItem)
            .WhereNotNull()
            .SelectMany(c => Observable.FromAsync(async () =>
            {
                _currentActiveGroupVm?.Deactivate();

                if (!_groupCache.TryGetValue(c.Id, out var vm))
                {
                    var group = await LoadGroupAsync(c.Id);

                    vm = _viewModelFactory.CreateGroup(
                        _userSession, 
                        group!);
                    
                    _groupCache.Add(
                        c.Id, vm);

                    if(!vm.IsGroupJoined)
                        await vm.JoinToGroupAsync(c.Id);
                    
                    MessageBus.Current.SendMessage(
                        Unit.Default,
                        MessageTokens.LoadGroupMessages);
                }

                _currentActiveGroupVm = vm;
                
                MessageBus.Current.SendMessage(
                    vm, 
                    MessageTokens.GroupPicked);
                
                await vm.Activate();
            }))
            .Subscribe()
            .DisposeWith(_disposables);
    }
}