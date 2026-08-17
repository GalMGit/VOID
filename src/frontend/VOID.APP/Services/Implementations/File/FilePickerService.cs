using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using VOID.APP.Services.Interfaces.IFile;
using VOID.APP.Views.Window;
using VOID.Shared.Contracts.Enums.Messages;

namespace VOID.APP.Services.Implementations.File;

public class FilePickerService(
    IServiceProvider serviceProvider
    ) : IFilePickerService
{
    public async Task<IStorageFile?> PickMediaFileAsync(string title = "Выберите файл")
    {
        var topLevel = serviceProvider.GetRequiredService<MainWindow>();
        if (topLevel is null) return null;
            
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Медиафайлы")
                {
                    Patterns = new[]
                    {
                        "*.png", "*.jpg", "*.jpeg", "*.gif",
                        "*.mp4", "*.avi", "*.mov", "*.mkv", "*.wmv", "*.flv"
                    }
                },
                new FilePickerFileType("Изображения")
                {
                    Patterns = ["*.png", "*.jpg", "*.jpeg", "*.gif"]
                },
                new FilePickerFileType("Видео")
                {
                    Patterns = ["*.mp4", "*.avi", "*.mov", "*.mkv", "*.wmv", "*.flv"]
                }
            ]
        });
            
        return files.Any() ? files[0] : null;
    }

    public async Task<IStorageFile?> PickImageFileAsync(
        string title = "Выберите изображение")
    {
        var topLevel = serviceProvider.GetRequiredService<MainWindow>();
        if (topLevel is null) return null;
            
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Изображения")
                {
                    Patterns = ["*.png", "*.jpg", "*.jpeg", "*.gif"]
                }
            ]
        });
            
        return files.Any() ? files[0] : null;
    }

    public async Task<string?> ValidateFileSizeAsync(
        IStorageFile file, 
        ulong maxSizeInBytes)
    {
        if (file is null) return "Файл не выбран";
            
        var fileInfo = await file.GetBasicPropertiesAsync();
        var fileSize = fileInfo.Size;
            
        if (fileSize > maxSizeInBytes)
        {
            var sizeLimitMb = maxSizeInBytes / (1024 * 1024);
            return $"Максимальный размер файла {sizeLimitMb} МБ";
        }
            
        return null;
    }

    public MessageType GetMessageTypeByExtension(string extension)
    {
        var imageExtensions = new[] { ".png", ".jpg", ".jpeg" };
        var videoExtensions = new[] { ".mp4", ".avi", ".mov", ".mkv", ".wmv", ".flv" };
            
        if (string.IsNullOrEmpty(extension))
            return MessageType.File;
                
        extension = extension.ToLower();
            
        if (extension == ".gif")
            return MessageType.Gif;
        if (imageExtensions.Contains(extension))
            return MessageType.Image;
        if (videoExtensions.Contains(extension))
            return MessageType.Video;
                
        return MessageType.File;
    }

    public ulong GetMaxFileSize(MessageType messageType)
    {
        return messageType switch
        {
            MessageType.Image or MessageType.Gif => 6 * 1024 * 1024,
            MessageType.Video => 50 * 1024 * 1024,
            _ => 6 * 1024 * 1024
        };
    }
}