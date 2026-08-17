using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using Microsoft.AspNetCore.SignalR.Client;
using VOID.APP.Models.Messages;
using VOID.Shared.Contracts.SignalRTokens;
using ReactiveUI;
using VOID.APP.Models.Chat;
using VOID.APP.Models.Group;
using VOID.APP.Models.Navigation;
using VOID.APP.Models.User;
using VOID.APP.ViewModels.Pages.Chat.CurrentChat;
using VOID.APP.ViewModels.Pages.Group.CurrentGroup;

namespace VOID.APP.ViewModels.Pages.Layout;

public partial class LayoutViewModel
{

    private void SetupSignalRHandlers()
    {
        _hubSubscriptions.Add(_hubConnection.On<int>(SignalRTokens.ConnectionInfo, HandleConnectionsEvent));
    }

    private void HandleConnectionsEvent(int count)
        => TotalConnections = count;

    private void SetupMessages()
    {
        this.WhenAnyValue(x => x.SelectedListItem)
            .Subscribe(x => CurrentListContent = x)
            .DisposeWith(_disposables);

        MessageBus.Current.Listen<CurrentChatViewModel>(MessageTokens.ChatPicked)
            .Subscribe(async c =>
            {
                CurrentChatContent = null;
                CurrentChatContent = c;

                MessageBus.Current.SendMessage(
                    Unit.Default,
                    MessageTokens.ClearSelectionGroupList);

                await Dispatcher.UIThread.InvokeAsync(
                    () => { },
                    DispatcherPriority.Loaded);

                await Dispatcher.UIThread.InvokeAsync(
                    () => { },
                    DispatcherPriority.Render);

                MessageBus.Current.SendMessage(
                    Unit.Default,
                    MessageTokens.ScrollToBottom);
            })
            .DisposeWith(_disposables);

        MessageBus.Current.Listen<CurrentGroupViewModel>(MessageTokens.GroupPicked)
            .Subscribe(async c =>
            {
                CurrentChatContent = null;
                CurrentChatContent = c;

                MessageBus.Current.SendMessage(
                    Unit.Default,
                    MessageTokens.ClearSelectionList);

                await Dispatcher.UIThread.InvokeAsync(
                    () => { },
                    DispatcherPriority.Loaded);

                await Dispatcher.UIThread.InvokeAsync(
                    () => { },
                    DispatcherPriority.Render);

                MessageBus.Current.SendMessage(
                    Unit.Default,
                    MessageTokens.ScrollToBottom);
            })
            .DisposeWith(_disposables);

        MessageBus.Current.Listen<Unit>(MessageTokens.ClearSelectionCurrent)
            .Subscribe(_ => CurrentChatContent = null)
            .DisposeWith(_disposables);

        MessageBus.Current.Listen<SearchUserResponse>(MessageTokens.SearchUser)
            .Subscribe(u => SelectedUser = u)
            .DisposeWith(_disposables);

        MessageBus.Current.Listen<FullChatModel>(MessageTokens.OpenInterlocutorProfile)
            .SelectMany(x => Observable.FromAsync(async () =>
            {
                var profile = _viewModelFactory.CreateInterlocutorProfile(x);
                if (profile == null) return;

                InterlocutorProfileContent = profile;
                await OpenDialogAsync(profile);
            }))
            .Subscribe()
            .DisposeWith(_disposables);


        this.WhenAnyValue(x => x.SearchText)
            .Subscribe(_ =>
            {
                CurrentListContent = ListPages[0];
                SelectedListItem = ListPages[0];
                SearchResults.Clear();
                _lastSearchText = null;
            });

        MessageBus.Current.Listen<Unit>(MessageTokens.ClearLastSearchText)
            .Subscribe(_ => _lastSearchText = null)
            .DisposeWith(_disposables);

        this.WhenAnyValue(x => x.SelectedUser)
            .Where(user => user != null)
            .SelectMany(u => Observable.FromAsync(async () =>
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    SearchResults.Clear();
                    SearchText = string.Empty;

                    MessageBus.Current.SendMessage(
                        u,
                        MessageTokens.CreateChat);

                    MessageBus.Current.SendMessage(
                        Unit.Default,
                        MessageTokens.ClearLastSearchText);
                });
            }))
            .Subscribe()
            .DisposeWith(_disposables);


        MessageBus.Current.Listen<Unit>(MessageTokens.AvatarHasDeleted)
            .Subscribe(_ => AvatarImage = null)
            .DisposeWith(_disposables);

        MessageBus.Current.Listen<UserAuthModel>(MessageTokens.AvatarLoaded)
            .Subscribe(c =>
            {
                AvatarImage = c.AvatarUrl;
                CurrentName = c.Name;
            })
            .DisposeWith(_disposables);

        MessageBus.Current.SendMessage(Unit.Default,
            MessageTokens.LoadLists);

        MessageBus.Current.SendMessage(Unit.Default,
            MessageTokens.LoadGroups);

        MessageBus.Current.Listen<string>(MessageTokens.OpenImageDialog)
            .SelectMany(image => Observable.FromAsync(async () =>
            {
                var dialog = _viewModelFactory.CreateImageModal(image);
                await OpenDialogAsync(dialog);
            }))
            .Subscribe()
            .DisposeWith(_disposables);

        MessageBus.Current.Listen<string>(MessageTokens.OpenVideoDialog)
            .SelectMany(video => Observable.FromAsync(async () =>
            {
                var dialog = _viewModelFactory.CreateVideoModal(video);
                await OpenDialogAsync(dialog);
            }))
            .Subscribe()
            .DisposeWith(_disposables);

        MessageBus.Current.Listen<GroupModel>(MessageTokens.OpenAddMemberDialog)
            .SelectMany(g => Observable.FromAsync(async () =>
            {
                var dialog = _viewModelFactory.CreateAddMember(
                    g);

                await OpenDialogAsync(dialog);
            }))
            .Subscribe()
            .DisposeWith(_disposables);

        MessageBus.Current.Listen<FullGroupModel>(MessageTokens.OpenEditGroupDialog)
            .SelectMany(g => Observable.FromAsync(async () =>
            {
                var dialog = _viewModelFactory.CreateEditGroup(g);
                await OpenDialogAsync(dialog);
            }))
            .Subscribe()
            .DisposeWith(_disposables);

        MessageBus.Current.Listen<Unit>(MessageTokens.CloseDialog)
            .Subscribe(_ => CloseDialog())
            .DisposeWith(_disposables);

        MessageBus.Current.Listen<string>(MessageTokens.NameUpdated)
            .Subscribe(s => CurrentName = s)
            .DisposeWith(_disposables);

        MessageBus.Current.Listen<Unit>(MessageTokens.GroupCreated)
            .Subscribe(_ =>
            {
                CloseDialog();
                SelectedListItem = ListPages[1];
                CurrentListContent = ListPages[1];
            })
            .DisposeWith(_disposables);
    }
}