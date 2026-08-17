using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using VOID.Shared.Contracts.Enums.Messages;

namespace VOID.APP.Services.Interfaces.IFile;

public interface IFilePickerService
{
    Task<IStorageFile?> PickMediaFileAsync(string title = "Выберите файл");
    Task<IStorageFile?> PickImageFileAsync(string title = "Выберите изображение");
    Task<string?> ValidateFileSizeAsync(IStorageFile file, ulong maxSizeInBytes);
    MessageType GetMessageTypeByExtension(string extension);
    ulong GetMaxFileSize(MessageType messageType);
}