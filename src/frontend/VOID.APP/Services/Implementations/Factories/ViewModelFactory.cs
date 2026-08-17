using System;
using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using VOID.APP.Models.Chat;
using VOID.APP.Models.Group;
using VOID.APP.Models.User;
using VOID.APP.Services.Interfaces;
using VOID.APP.ViewModels.Modals.Image;
using VOID.APP.ViewModels.Modals.Video;
using VOID.APP.ViewModels.Pages.Auth.AuthLayout;
using VOID.APP.ViewModels.Pages.Auth.ConfirmEmail;
using VOID.APP.ViewModels.Pages.Auth.Login;
using VOID.APP.ViewModels.Pages.Auth.Register;
using VOID.APP.ViewModels.Pages.Chat.CurrentChat;
using VOID.APP.ViewModels.Pages.Chat.ListChats;
using VOID.APP.ViewModels.Pages.Group.AddMember;
using VOID.APP.ViewModels.Pages.Group.CreateGroup;
using VOID.APP.ViewModels.Pages.Group.CurrentGroup;
using VOID.APP.ViewModels.Pages.Group.EditGroup;
using VOID.APP.ViewModels.Pages.Group.ListGroups;
using VOID.APP.ViewModels.Pages.Layout;
using VOID.APP.ViewModels.Pages.Layout.Search;
using VOID.APP.ViewModels.Pages.Profile;

namespace VOID.APP.Services.Implementations.Factories;

public class ViewModelFactory(IServiceProvider serviceProvider) : IViewModelFactory
{
    public AuthLayoutViewModel CreateAuthLayout()
    {
        var factory = serviceProvider.GetRequiredService<Func<AuthLayoutViewModel>>();
        return factory();
    }

    public ImageWindowViewModel CreateImageModal(string imageUrl)
    {
        var factory = serviceProvider.GetRequiredService<Func<string, ImageWindowViewModel>>();
        return factory(imageUrl);
    }

    public CurrentChatViewModel CreateChat(UserSession userSession, FullChatModel fullChatModel)
    {
        var factory = serviceProvider.GetRequiredService<Func<UserSession, FullChatModel, CurrentChatViewModel>>();
        return factory(userSession, fullChatModel);
    }

    public InterlocutorProfileViewModel CreateInterlocutorProfile(FullChatModel chatModel)
    {
        var factory = serviceProvider.GetRequiredService<Func<FullChatModel, InterlocutorProfileViewModel>>();
        return factory(chatModel);
    }

    public LayoutViewModel CreateLayout(UserSession userSession)
    {
        var factory = serviceProvider.GetRequiredService<Func<UserSession, LayoutViewModel>>();
        return factory(userSession);
    }

    public VideoWindowViewModel CreateVideoModal(string videoUrl)
    {
        var factory = serviceProvider.GetRequiredService<Func<string, VideoWindowViewModel>>();
        return factory(videoUrl);
    }

    public CreateGroupViewModel CreateGroupModal()
    {
        var factory = serviceProvider.GetRequiredService<Func<CreateGroupViewModel>>();
        return factory();
    }

    public ListGroupsViewModel CreateListGroups(UserSession userSession)
    {
        var factory = serviceProvider.GetRequiredService<Func<UserSession, ListGroupsViewModel>>();
        return factory(userSession);
    }

    public CurrentGroupViewModel CreateGroup(UserSession userSession, FullGroupModel fullGroupModel)
    {
        var factory = serviceProvider.GetRequiredService<Func<UserSession, FullGroupModel, CurrentGroupViewModel>>();
        return factory(userSession, fullGroupModel);
    }

    public AddMemberViewModel CreateAddMember(GroupModel groupModel)
    {
        var factory = serviceProvider.GetRequiredService<Func<GroupModel, AddMemberViewModel>>();
        return factory(groupModel);
    }

    public ListChatsViewModel CreateListChats(UserSession userSession)
    {
        var factory = serviceProvider.GetRequiredService<Func<UserSession, ListChatsViewModel>>();
        return factory(userSession);
    }

    public LoginViewModel CreateLogin()
    {
        var factory = serviceProvider.GetRequiredService<Func<LoginViewModel>>();
        return factory();
    }

    public ProfileViewModel CreateProfile(UserSession userSession)
    {
        var factory = serviceProvider.GetRequiredService<Func<UserSession, ProfileViewModel>>();
        return factory(userSession);
    }

    public RegisterViewModel CreateRegister()
    {
        var factory = serviceProvider.GetRequiredService<Func<RegisterViewModel>>();
        return factory();
    }

    public SearchListViewModel CreateSearchList(ObservableCollection<SearchUserResponse> searchUsers)
    {
        var factory = serviceProvider.GetRequiredService<Func<ObservableCollection<SearchUserResponse>, SearchListViewModel>>();
        return factory(searchUsers);
    }

    public EditGroupViewModel CreateEditGroup(FullGroupModel groupModel)
    {
        var factory = serviceProvider.GetRequiredService<Func<FullGroupModel, EditGroupViewModel>>();
        return factory(groupModel);
    }

    public ConfirmEmailViewModel CreateConfirmEmail()
    {
        var factory = serviceProvider.GetRequiredService<Func<ConfirmEmailViewModel>>();
        return factory();
    }
}