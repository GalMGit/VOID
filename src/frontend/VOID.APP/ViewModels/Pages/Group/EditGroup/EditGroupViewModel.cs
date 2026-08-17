using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using MsBox.Avalonia;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using VOID.APP.Models.Group;
using VOID.APP.Models.Navigation;
using VOID.APP.Services.Interfaces.IFile;
using VOID.APP.Services.Interfaces.IGroup;
using VOID.APP.Services.Interfaces.IImage;
using VOID.APP.ViewModels.Base.ModalBase;


namespace VOID.APP.ViewModels.Pages.Group.EditGroup;

public partial class EditGroupViewModel : ModalViewModelBase
{
    private readonly IGroupService _groupService;
    private readonly IFilePickerService _filePickerService;
    private readonly IUserImageService _imageService;
    
    public FullGroupModel CurrentGroup { get; set; }

    public EditGroupViewModel(
        FullGroupModel currentGroup,
        IGroupService groupService,
        IFilePickerService filePickerService,
        IUserImageService imageService
        )
    {
        CurrentGroup = currentGroup;
        _filePickerService = filePickerService;
        _imageService = imageService;
        _groupService = groupService;
    }

    [ReactiveCommand]
    private async Task DeleteMemberAsync(GroupMemberModel member)
        => await _groupService.DeleteMemberFromGroupAsync(
            member.GroupId, 
            member.MemberId);
    
    [ReactiveCommand]
    private async Task UploadGroupImageAsync()
    {
        var file = await _filePickerService.PickImageFileAsync(
            "Выберите картинку");
        
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

        await _imageService.UpdateGroupImageAsync(content, CurrentGroup.Id);
    }
}