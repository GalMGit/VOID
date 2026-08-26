using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using VOID.APP.Models.Link;
using VOID.APP.Services.Mappers;
using VOID.APP.Models.Chat;
using VOID.APP.Models.Group;
using VOID.APP.Models.User;
using VOID.APP.Services.Implementations;
using VOID.APP.Services.Implementations.Audio;
using VOID.APP.Services.Implementations.Auth;
using VOID.APP.Services.Implementations.Chat;
using VOID.APP.Services.Implementations.Factories;
using VOID.APP.Services.Implementations.File;
using VOID.APP.Services.Implementations.Groups;
using VOID.APP.Services.Implementations.Image;
using VOID.APP.Services.Implementations.INotify;
using VOID.APP.Services.Implementations.Link;
using VOID.APP.Services.Implementations.Message;
using VOID.APP.Services.Implementations.Settings;
using VOID.APP.Services.Implementations.User;
using VOID.APP.Services.Interfaces;
using VOID.APP.Services.Interfaces.IAudio;
using VOID.APP.Services.Interfaces.IAuth;
using VOID.APP.Services.Interfaces.IChat;
using VOID.APP.Services.Interfaces.IFile;
using VOID.APP.Services.Interfaces.IGroup;
using VOID.APP.Services.Interfaces.IImage;
using VOID.APP.Services.Interfaces.ILink;
using VOID.APP.Services.Interfaces.IMessage;
using VOID.APP.Services.Interfaces.INotify;
using VOID.APP.Services.Interfaces.ISettings;
using VOID.APP.Services.Interfaces.IUser;
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
using VOID.APP.ViewModels.Pages.Settings;
using VOID.APP.ViewModels.Pages.Settings.ChangePassword;
using VOID.APP.ViewModels.Pages.Settings.Menu;
using VOID.APP.ViewModels.Window;
using VOID.APP.Views.Window;

namespace VOID.APP.DI;

public static class DIConfig
{
    public static void ConfigureViewModels(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddSingleton<MainWindow>();
        services.AddSingleton<Window>(x => x.GetRequiredService<MainWindow>());
        services.AddSingleton<IAuthErrorHandler, AuthErrorHandler>();
        services.AddSingleton<ITokenStorageService, TokenStorageService>();
        services.AddSingleton<ITokenService, TokenService>();

        services.AddSingleton<RefreshTokenManager>();

        services.AddSingleton(sp =>
        {
            var tokenService = sp.GetRequiredService<ITokenService>();
            var errorHandler = sp.GetRequiredService<IAuthErrorHandler>();
            var refreshManager = sp.GetRequiredService<RefreshTokenManager>();

            var authHandler = new AuthHandler(tokenService, errorHandler, refreshManager)
            {
                InnerHandler = new HttpClientHandler()
                {
                    MaxRequestContentBufferSize = 50 * 1024 * 1024,
                    AllowAutoRedirect = true
                }
            };

            return new HttpClient(authHandler)
            {
                BaseAddress = new Uri(Urls.BaseApiUrl),
                Timeout = TimeSpan.FromMinutes(3)
            };
        });

        services.AddSingleton(sp =>
        {
            var tokenService = sp.GetRequiredService<ITokenService>();
            return new HubConnectionBuilder()
                .WithUrl(Urls.BaseHubUrl, options =>
                {
                    options.AccessTokenProvider = () =>
                        Task.FromResult(tokenService.AccessToken);

                    options.WebSocketConfiguration = sockets =>
                    {
                        sockets.SetBuffer(10 * 1024 * 1024, 10 * 1024 * 1024);
                    };
                })
                .WithAutomaticReconnect()
                .Build();
        });

        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<IUserService, UserService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IChatService, ChatService>();
        services.AddSingleton<IGroupService, GroupService>();

        services.AddSingleton<IMessageService, MessageService>();
        services.AddSingleton<IAudioRecordingService, SoundFlowAudioRecordingService>();
        services.AddSingleton<IAudioPlaybackService, SoundFlowAudioPlaybackService>();
        services.AddSingleton<IUserImageService, UserImageService>();
        services.AddSingleton<ILinkPreviewService, LinkPreviewService>();

        services.AddSingleton<IFilePickerService, FilePickerService>();

        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<IDialogService, DialogService>();

        services.AddTransient<ProfileViewModel>();
        services.AddTransient<CreateGroupViewModel>();
        services.AddTransient<CurrentGroupViewModel>();
        services.AddTransient<AddMemberViewModel>();
        services.AddTransient<ListGroupsViewModel>();
        services.AddTransient<InterlocutorProfileViewModel>();
        services.AddSingleton<ImageWindowViewModel>();
        services.AddSingleton<VideoWindowViewModel>();
        services.AddTransient<CurrentChatViewModel>();
        services.AddTransient<ListChatsViewModel>();

        services.AddTransient<SearchListViewModel>();
        services.AddTransient<LayoutViewModel>();
        services.AddTransient<AuthLayoutViewModel>();
        services.AddTransient<EditGroupViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<ConfirmEmailViewModel>();

        services.AddTransient<Func<UserSession, ProfileViewModel>>(sp =>
            session => ActivatorUtilities.CreateInstance<ProfileViewModel>(sp, session));

        services.AddTransient<Func<CreateGroupViewModel>>(sp =>
            () => ActivatorUtilities.CreateInstance<CreateGroupViewModel>(sp));

        services.AddTransient<Func<GroupModel, AddMemberViewModel>>(sp =>
            groupModel => ActivatorUtilities.CreateInstance<AddMemberViewModel>(sp, groupModel));

        services.AddTransient<Func<UserSession, ListGroupsViewModel>>(sp =>
            session => ActivatorUtilities.CreateInstance<ListGroupsViewModel>(sp, session));

        services.AddTransient<Func<FullChatModel, InterlocutorProfileViewModel>>(sp =>
            chat => ActivatorUtilities.CreateInstance<InterlocutorProfileViewModel>(sp, chat));

        services.AddTransient<Func<FullGroupModel, EditGroupViewModel>>(sp =>
            group => ActivatorUtilities.CreateInstance<EditGroupViewModel>(sp, group));

        services.AddTransient<Func<UserSession, FullChatModel, CurrentChatViewModel>>(sp =>
            (session, chat) => ActivatorUtilities.CreateInstance<CurrentChatViewModel>(sp, session, chat));

        services.AddTransient<Func<UserSession, FullGroupModel, CurrentGroupViewModel>>(sp =>
            (session, group) => ActivatorUtilities.CreateInstance<CurrentGroupViewModel>(sp, session, group));

        services.AddTransient<Func<UserSession, ListChatsViewModel>>(sp =>
            session => ActivatorUtilities.CreateInstance<ListChatsViewModel>(sp, session));

        services.AddTransient<Func<ObservableCollection<SearchUserResponse>, SearchListViewModel>>(sp =>
            results => ActivatorUtilities.CreateInstance<SearchListViewModel>(sp, results));

        services.AddTransient<Func<UserSession, LayoutViewModel>>(sp =>
            session => ActivatorUtilities.CreateInstance<LayoutViewModel>(sp, session));

        services.AddTransient<Func<string, ImageWindowViewModel>>(sp =>
            imageUrl => ActivatorUtilities.CreateInstance<ImageWindowViewModel>(sp, imageUrl));

        services.AddTransient<Func<Guid, VideoWindowViewModel>>(sp =>
            messageId => ActivatorUtilities.CreateInstance<VideoWindowViewModel>(sp, messageId));

        services.AddTransient<Func<AuthLayoutViewModel>>(sp =>
            () => ActivatorUtilities.CreateInstance<AuthLayoutViewModel>(sp));

        services.AddTransient<Func<LoginViewModel>>(sp =>
            () => ActivatorUtilities.CreateInstance<LoginViewModel>(sp));

        services.AddTransient<Func<RegisterViewModel>>(sp =>
            () => ActivatorUtilities.CreateInstance<RegisterViewModel>(sp));

        services.AddTransient<Func<ConfirmEmailViewModel>>(sp =>
            () => ActivatorUtilities.CreateInstance<ConfirmEmailViewModel>(sp));
        
        services.AddTransient<Func<SettingsViewModel>>(sp =>
            () => ActivatorUtilities.CreateInstance<SettingsViewModel>(sp));
        
        services.AddTransient<Func<SettingsMenuViewModel>>(sp =>
            () => ActivatorUtilities.CreateInstance<SettingsMenuViewModel>(sp));
        
        services.AddTransient<Func<ChangePasswordViewModel>>(sp =>
            () => ActivatorUtilities.CreateInstance<ChangePasswordViewModel>(sp));
        
        services.AddTransient<Func<ResetPasswordViewModel>>(sp =>
            () => ActivatorUtilities.CreateInstance<ResetPasswordViewModel>(sp));

        services.AddSingleton<IViewModelFactory, ViewModelFactory>();
    }
}