using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Threading;
using Microsoft.AspNetCore.SignalR.Client;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using VOID.APP.Models.Group;
using VOID.APP.Models.Navigation;
using VOID.APP.Models.User;
using VOID.APP.Services.Interfaces;
using VOID.APP.Services.Interfaces.IGroup;
using VOID.APP.ViewModels.Base.PageBase;
using VOID.APP.ViewModels.Pages.Group.CurrentGroup;

namespace VOID.APP.ViewModels.Pages.Group.ListGroups;

public partial class ListGroupsViewModel : PageViewModelBase
{
    private readonly HubConnection _hubConnection;
    private readonly IGroupService _groupService;
    private readonly Guid _currentUserId;
    private readonly UserSession _userSession;
    private readonly IViewModelFactory _viewModelFactory;
    [Reactive] public partial GroupModel? SelectedGroupItem { get; set; }
    public ObservableCollection<GroupModel> Groups { get; set; } = [];
    private readonly Dictionary<Guid, CurrentGroupViewModel> _groupCache = [];
    private CurrentGroupViewModel? _currentActiveGroupVm;
    private readonly List<IDisposable> _hubSubscriptions = [];

    [Reactive] public partial int CurrentPage { get; set; }
    [Reactive] public partial bool HasNextPages { get; set; }
    [Reactive] public partial int TotalPages { get; set; }
    [Reactive] public partial bool IsRefreshing { get; set; }
    [Reactive] public partial bool IsLoadingMore { get; set; }

    private const int PageSize = 15;

    public ListGroupsViewModel(
        UserSession userSession,
        IViewModelFactory viewModelFactory,
        HubConnection hubConnection,
        IGroupService groupService)
    {
        Title = "Группы";
        _userSession = userSession;
        _currentUserId = userSession.UserId;
        _hubConnection = hubConnection;
        _viewModelFactory = viewModelFactory;
        _groupService = groupService;

        SetupMessages();
        SetupSignalRHandlers();
    }

    private async Task LoadGroups(int pageNumber)
    {
        if (IsLoadingMore) return;
        IsLoadingMore = true;
        try
        {
            var result = await _groupService.GetGroupsForUserAsync(
                pageNumber, 
                PageSize);

            if (result is not null)
            {
                if (pageNumber == 1)
                {
                    Groups.Clear();
                    foreach (var chat in result.Items)
                    {
                        Groups.Add(chat);
                        
                        await _hubConnection.InvokeAsync(
                            SignalREvents.JoinToGroupEvent,
                            chat.Id);
                        
                        MessageBus.Current.SendMessage(
                            true, 
                            MessageTokens.GroupJoined);

                        chat.CurrentUserId = _currentUserId;
                    }
                }
                else
                {
                    foreach (var chat in result.Items
                                 .Where(chat => Groups
                                     .All(x => x.Id != chat.Id)))
                    {
                        Groups.Add(chat);
                        
                        chat.CurrentUserId = _currentUserId;
                        
                        await _hubConnection.InvokeAsync(
                            SignalREvents.JoinToGroupEvent,
                            chat.Id);
                        
                        MessageBus.Current.SendMessage(
                            true, 
                            MessageTokens.GroupJoined);
                    }

                }

                CurrentPage = result.PageNumber;
                TotalPages = result.TotalPages;
                HasNextPages = result.HasNext;
            }
        }
        finally
        {
            IsLoadingMore = false;
        }
    }

    private async Task<FullGroupModel?> LoadGroupAsync(Guid groupId)
        => await _groupService.GetGroupByIdAsync(groupId);

    [ReactiveCommand]
    private async Task LeaveFromGroupAsync(GroupModel group)
    {
        await _groupService.LeaveFromGroupAsync(group.Id);

        await _hubConnection.InvokeAsync(
            SignalREvents.RemoveFromGroupEvent,
            group.Id);

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var groupForLeave = Groups
                .FirstOrDefault(x => x.Id == group.Id);
            
            if (groupForLeave is not null)
            {
                Groups.Remove(groupForLeave);
                
                MessageBus.Current.SendMessage(
                    Unit.Default,
                    MessageTokens.ClearSelectionCurrent);
            }
        });
    }

    [ReactiveCommand]
    private async Task DeleteGroupAsync(GroupModel group)
        => await _groupService.DeleteGroupAsync(group.Id);
}