using System;
using System.Collections.ObjectModel;
using VOID.APP.Views.Pages.Auth.AuthLayout;
using VOID.APP.Views.Pages.Chat.CurrentChat;
using VOID.APP.Models.Chat;
using VOID.APP.Models.Group;
using VOID.APP.Models.User;
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

namespace VOID.APP.Services.Interfaces;

public interface IViewModelFactory
{
    ListChatsViewModel CreateListChats(UserSession userSession);
    ProfileViewModel CreateProfile(UserSession userSession);
    SearchListViewModel CreateSearchList(ObservableCollection<SearchUserResponse> searchUsers);
    InterlocutorProfileViewModel CreateInterlocutorProfile(FullChatModel chatModel);
    ImageWindowViewModel CreateImageModal(string imageUrl);
    CurrentChatViewModel CreateChat(UserSession userSession, FullChatModel fullChatModel);
    RegisterViewModel CreateRegister();
    LoginViewModel CreateLogin();
    AuthLayoutViewModel CreateAuthLayout();
    LayoutViewModel CreateLayout(UserSession userSession);
    VideoWindowViewModel CreateVideoModal(string videoUrl);
    CreateGroupViewModel CreateGroupModal();
    ListGroupsViewModel CreateListGroups(UserSession userSession);
    CurrentGroupViewModel CreateGroup(UserSession userSession, FullGroupModel fullGroupModel);
    AddMemberViewModel CreateAddMember(GroupModel groupModel);
    ConfirmEmailViewModel CreateConfirmEmail();
    EditGroupViewModel CreateEditGroup(FullGroupModel groupModel);
}