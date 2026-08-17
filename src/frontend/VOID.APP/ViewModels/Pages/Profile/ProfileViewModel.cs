using System;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using MsBox.Avalonia;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using VOID.APP.Models.Navigation;
using VOID.APP.Models.User;
using VOID.APP.Services.Interfaces.IFile;
using VOID.APP.Services.Interfaces.IImage;
using VOID.APP.Services.Interfaces.IUser;
using VOID.APP.ViewModels.Base.ModalBase;

namespace VOID.APP.ViewModels.Pages.Profile;

public partial class ProfileViewModel : ModalViewModelBase
{
    private readonly IUserService _userService;
    private readonly IFilePickerService _filePickerService;
    private readonly Guid _currentUserId;
    private readonly IUserImageService _imageService;

    [Reactive] public partial UserAuthModel UserModel { get; set; }
    [Reactive] public partial bool EditBoxIsReadOnly { get; set; }
    [Reactive] public partial bool IsEditing { get; set; }
    [Reactive] public partial string NameTempText { get; set; }
    [Reactive] public partial string AboutMeTempText { get; set; }

    public ReactiveCommand<Unit, Unit> LogoutCommand { get; }

    public ProfileViewModel(
        UserSession userSession,
        IUserService userService,
        IUserImageService imageService,
        IFilePickerService filePickerService)
    {
        _userService = userService;
        _imageService = imageService;
        _currentUserId = userSession.UserId;
        _filePickerService = filePickerService;

        EditBoxIsReadOnly = true;

        LogoutCommand = ReactiveCommand.Create(() =>
                MessageBus.Current.SendMessage(
                    Unit.Default,
                    MessageTokens.Logout))
            .DisposeWith(_disposables);

        SetupMessages();
    }

    private void SetupMessages()
    {
        MessageBus.Current.Listen<Unit>(MessageTokens.LoadAvatars)
            .SelectMany(_ => Observable.FromAsync(async ()
                => await LoadProfileInfoAsync()))
            .Subscribe()
            .DisposeWith(_disposables);

        MessageBus.Current.Listen<Unit>(MessageTokens.AvatarHasDeleted)
            .Subscribe(_ => UserModel.AvatarUrl = null)
            .DisposeWith(_disposables);
    }

    [ReactiveCommand]
    private void StartUpdateProfile()
    {
        IsEditing = true;
        EditBoxIsReadOnly = false;
    }

    [ReactiveCommand]
    private void CancelUpdateProfile()
    {
        IsEditing = false;
        EditBoxIsReadOnly = true;
        NameTempText = UserModel.Name;
        AboutMeTempText = UserModel.AboutMe ?? string.Empty;
    }

    [ReactiveCommand]
    private async Task UpdateProfileAsync()
    {
        if (AboutMeTempText == UserModel.AboutMe && NameTempText == UserModel.Name)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Ошибка",
                "Данные должны отличаться");

            await box.ShowAsync();
            return;
        }

        if (NameTempText != UserModel.Name)
        {
            UserModel.Name = NameTempText;
        }

        if (AboutMeTempText != UserModel.AboutMe)
        {
            UserModel.AboutMe = AboutMeTempText;
        }


        var updatedProfile = await _userService.UpdateProfileAsync(UserModel);

        if (updatedProfile is not null)
        {
            UserModel = updatedProfile;
            IsEditing = false;
            EditBoxIsReadOnly = true;

            MessageBus.Current.SendMessage(
                updatedProfile.Name,
                MessageTokens.NameUpdated);
        }
    }

    [ReactiveCommand]
    private async Task DeleteAvatarAsync()
    {
        if (UserModel.AvatarUrl is null) return;

        await _imageService.DeleteAvatarAsync();

        MessageBus.Current.SendMessage(
            Unit.Default,
            MessageTokens.AvatarHasDeleted);
    }

    private async Task LoadProfileInfoAsync()
    {
        UserModel = await _userService.GetProfileInfoAsync(_currentUserId)!;
        NameTempText = UserModel.Name;
        AboutMeTempText = UserModel.AboutMe ?? string.Empty;

        MessageBus.Current.SendMessage(
            UserModel,
            MessageTokens.AvatarLoaded);
    }

    [ReactiveCommand]
    private async Task UploadAvatarAsync()
    {
        var file = await _filePickerService.PickImageFileAsync(
            "Выберите аватар");

        if (file is null) return;

        var validationError = await _filePickerService.ValidateFileSizeAsync(
            file,
            6 * 1024 * 1024);

        if (validationError is not null)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Ошибка",
                validationError);

            await box.ShowAsync();
        }

        await using var stream = await file.OpenReadAsync();
        using var content = new MultipartFormDataContent();
        using var fileContent = new StreamContent(stream);
        content.Add(fileContent, "image", file.Name);

        var newImage = await _imageService.UploadAvatarAsync(content);
        if(newImage is null)
        {
            Console.WriteLine(newImage);
            return;
        }
        
        UserModel.AvatarUrl = newImage;

        MessageBus.Current.SendMessage(
            UserModel,
            MessageTokens.AvatarLoaded);
    }
}