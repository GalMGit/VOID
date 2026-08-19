using FFMpegCore;
using MimeDetective;
using VOID.Application.Abstractions.IServices.IMediaServices;
using VOID.Application.Exceptions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace VOID.Infrastructure.Storage;

public sealed class FileStorageService : IFileStorageService
{
    private readonly IPublicStorage _publicStorage;
    private readonly IPrivateStorage _privateStorage;
    private readonly IContentInspector _inspector;

    private static readonly HashSet<string> AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/gif",
        "video/mp4",
        "video/webm",
        "audio/wav"
    ];

    public FileStorageService(
        IPublicStorage publicStorage,
        IPrivateStorage privateStorage)
    {
        _publicStorage = publicStorage;
        _privateStorage = privateStorage;

        _inspector = new ContentInspectorBuilder
        {
            Definitions =
            [
                .. MimeDetective.Definitions.DefaultDefinitions.FileTypes.Images.JPEG(),
                .. MimeDetective.Definitions.DefaultDefinitions.FileTypes.Images.PNG(),
                .. MimeDetective.Definitions.DefaultDefinitions.FileTypes.Images.GIF(),
                .. MimeDetective.Definitions.DefaultDefinitions.FileTypes.Video.All(),
                .. MimeDetective.Definitions.DefaultDefinitions.FileTypes.Audio.WAV()
            ]
        }.Build();
    }

    #region Public Methods

    public Task<FileUploadResult> UploadAvatarAsync(
        UploadFile file,
        Guid userId,
        CancellationToken ct = default)
        => UploadProfileImageAsync(
            file,
            $"users/{userId}",
            "Avatar must be an image.",
            ct);

    public Task<FileUploadResult> UploadGroupImageAsync(
        UploadFile file,
        Guid groupId,
        CancellationToken ct = default)
        => UploadProfileImageAsync(
            file,
            $"groups/{groupId}",
            "Group Image must be an image.",
            ct);

    public async Task<FileUploadResult> UploadMessageMediaAsync(
        UploadFile file,
        Guid chatId,
        CancellationToken ct = default)
    {
        var detectedContentType = Validate(file.Stream);
        ValidateFileSize(file.Length, detectedContentType);

        var mediaKey = CreateMediaKey(chatId, detectedContentType);

        if (detectedContentType.StartsWith("video/"))
            return await UploadVideoAsync(file, chatId, mediaKey, detectedContentType, ct);

        if (detectedContentType.StartsWith("audio/"))
            return await UploadAudioAsync(file, mediaKey, detectedContentType, ct);

        if (detectedContentType.StartsWith("image/"))
            return await UploadImageWithThumbnailAsync(file, chatId, mediaKey, detectedContentType, ct);

        throw new ValidationException($"Unsupported content type: {detectedContentType}");
    }

    public Task DeleteChatMessagesFolderAsync(
        Guid chatId,
        CancellationToken ct = default)
        => _privateStorage.DeletePrefixAsync($"messages/{chatId}/", ct);

    public string GetAvatarUrl(string key)
        => _publicStorage.GetPublicUrl(key);

    public string GetMessageMediaUrl(string key, TimeSpan lifetime)
        => _privateStorage.GetPresignedUrl(key, lifetime);

    public async Task DeleteMediaAsync(
        string? mediaPath,
        string? thumbnailPath = null,
        CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(thumbnailPath))
            await _privateStorage.DeleteAsync(thumbnailPath, ct);

        if (!string.IsNullOrWhiteSpace(mediaPath))
            await _privateStorage.DeleteAsync(mediaPath, ct);
    }

    public async Task DeleteAvatarAsync(
        string? avatarPath,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(avatarPath))
            return;

        await _publicStorage.DeleteAsync(avatarPath, ct);
    }

    #endregion

    #region Private Upload Methods

    private async Task<FileUploadResult> UploadProfileImageAsync(
        UploadFile file,
        string folderPath,
        string errorMessage,
        CancellationToken ct)
    {
        ValidateFileSize(file.Length, StorageConstants.MaxImageSize);
        var detectedContentType = Validate(file.Stream);

        if (!detectedContentType.StartsWith("image/"))
            throw new ValidationException(errorMessage);

        await using var processedImage = await ProcessImageAsync(
            file.Stream,
            StorageConstants.ProfileImageWidth,
            StorageConstants.ProfileImageHeight,
            StorageConstants.ProfileImageQuality,
            ct);

        var key = $"{folderPath}/{Guid.NewGuid()}.jpg";
        const string contentType = "image/jpeg";

        await _publicStorage.UploadAsync(key, processedImage, contentType, ct);

        return new FileUploadResult(key, null, contentType);
    }

    private async Task<FileUploadResult> UploadVideoAsync(
        UploadFile file,
        Guid chatId,
        string mediaKey,
        string contentType,
        CancellationToken ct)
    {
        await using var memory = await file.Stream.ToMemoryStreamAsync(ct);
        await _privateStorage.UploadAsync(mediaKey, memory, contentType, ct);

        memory.Position = 0;
        await using var thumbnail = await CreateVideoThumbnailAsync(memory, ct);
        var thumbnailKey = CreateThumbnailKey(chatId);

        await _privateStorage.UploadAsync(thumbnailKey, thumbnail, "image/jpeg", ct);

        return new FileUploadResult(mediaKey, thumbnailKey, contentType);
    }

    private async Task<FileUploadResult> UploadAudioAsync(
        UploadFile file,
        string mediaKey,
        string contentType,
        CancellationToken ct)
    {
        file.Stream.Position = 0;
        await _privateStorage.UploadAsync(mediaKey, file.Stream, contentType, ct);

        return new FileUploadResult(mediaKey, null, contentType);
    }

    private async Task<FileUploadResult> UploadImageWithThumbnailAsync(
        UploadFile file,
        Guid chatId,
        string mediaKey,
        string contentType,
        CancellationToken ct)
    {
        await using var imageStream = await file.Stream.ToMemoryStreamAsync(ct);
        await _privateStorage.UploadAsync(mediaKey, imageStream, contentType, ct);

        string? thumbnailKey = null;

        if (contentType != "image/gif")
        {
            imageStream.Position = 0;
            await using var thumbnail = await ProcessImageAsync(
                imageStream,
                StorageConstants.ThumbnailWidth,
                StorageConstants.ThumbnailHeight,
                StorageConstants.ThumbnailQuality,
                ct);

            thumbnailKey = CreateThumbnailKey(chatId);
            await _privateStorage.UploadAsync(thumbnailKey, thumbnail, "image/jpeg", ct);
        }

        return new FileUploadResult(mediaKey, thumbnailKey, contentType);
    }

    #endregion

    #region Private Helper Methods

    private string Validate(Stream stream)
    {
        var matches = _inspector.Inspect(stream);
        stream.Position = 0;

        if (matches.Length == 0)
            throw new ValidationException("Invalid file type.");

        var mimeType = matches
            .OrderByDescending(m => m.Points)
            .First()
            .Definition.File.MimeType;

        if (mimeType is null || !AllowedContentTypes.Contains(mimeType))
            throw new ValidationException($"Unsupported content type: {mimeType ?? "unknown"}");

        return mimeType;
    }

    private void ValidateFileSize(long fileLength, string contentType)
    {
        var maxSize = contentType.StartsWith("video/")
            ? StorageConstants.MaxVideoSize
            : StorageConstants.MaxImageSize;

        ValidateFileSize(fileLength, maxSize);
    }

    private void ValidateFileSize(long fileLength, int maxSize)
    {
        if (fileLength > maxSize)
            throw new ValidationException($"Maximum size is {maxSize / 1024 / 1024} MB.");
    }

    private async Task<MemoryStream> ProcessImageAsync(
        Stream stream,
        int width,
        int height,
        int quality,
        CancellationToken ct)
    {
        stream.Position = 0;
        using var image = await Image.LoadAsync(stream, ct);

        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(width, height),
            Mode = ResizeMode.Crop
        }));

        var output = new MemoryStream();
        await image.SaveAsJpegAsync(output, new JpegEncoder { Quality = quality }, ct);
        output.Position = 0;

        return output;
    }

    private async Task<MemoryStream> CreateVideoThumbnailAsync(
        Stream videoStream,
        CancellationToken ct)
    {
        var tempFiles = new List<string>();

        try
        {
            var inputFile = CreateTempFile("mp4", tempFiles);
            var outputFile = CreateTempFile("jpg", tempFiles);

            videoStream.Position = 0;
            await using (var file = File.Create(inputFile))
            {
                await videoStream.CopyToAsync(file, ct);
            }

            ct.ThrowIfCancellationRequested();

            var process = FFMpegArguments
                .FromFileInput(inputFile)
                .OutputToFile(outputFile, overwrite: true,
                    options => options
                        .Seek(TimeSpan.FromSeconds(2))
                        .WithFrameOutputCount(1))
                .CancellableThrough(ct);

            var success = await process.ProcessAsynchronously();

            if (!success)
                throw new ValidationException("Failed to create video thumbnail");

            ct.ThrowIfCancellationRequested();

            return await ProcessImageAsync(
                File.OpenRead(outputFile),
                StorageConstants.VideoThumbnailWidth,
                StorageConstants.VideoThumbnailHeight,
                StorageConstants.VideoThumbnailQuality,
                ct);
        }
        finally
        {
            CleanupTempFiles(tempFiles);
        }
    }

    private string CreateTempFile(string extension, List<string> files)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.{extension}");
        files.Add(path);
        return path;
    }

    private void CleanupTempFiles(List<string> files)
    {
        foreach (var file in files.Where(File.Exists))
        {
            try
            {
                File.Delete(file);
            }
            catch
            {
                // Логирование ошибки удаления временного файла
            }
        }
    }

    private static string CreateMediaKey(Guid chatId, string contentType)
        => $"messages/{chatId}/{Guid.NewGuid()}{GetExtension(contentType)}";

    private static string CreateThumbnailKey(Guid chatId)
        => $"messages/{chatId}/{Guid.NewGuid()}_thumb.jpg";

    private static string GetExtension(string contentType)
    {
        return contentType switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/gif" => ".gif",
            "video/mp4" => ".mp4",
            "video/webm" => ".webm",
            "audio/wav" => ".wav",
            _ => throw new ValidationException($"Unsupported content type: {contentType}")
        };
    }

    #endregion
}

#region Helper Classes

internal static class StorageConstants
{
    public const int MaxImageSize = 6 * 1024 * 1024;
    public const int MaxVideoSize = 50 * 1024 * 1024;
    
    public const int ProfileImageWidth = 200;
    public const int ProfileImageHeight = 200;
    public const int ProfileImageQuality = 50;
    
    public const int ThumbnailWidth = 200;
    public const int ThumbnailHeight = 200;
    public const int ThumbnailQuality = 45;
    
    public const int VideoThumbnailWidth = 300;
    public const int VideoThumbnailHeight = 200;
    public const int VideoThumbnailQuality = 80;
}

internal static class StreamExtensions
{
    public static async Task<MemoryStream> ToMemoryStreamAsync(
        this Stream stream,
        CancellationToken ct = default)
    {
        var memoryStream = new MemoryStream();
        stream.Position = 0;
        await stream.CopyToAsync(memoryStream, ct);
        memoryStream.Position = 0;
        return memoryStream;
    }
}

#endregion