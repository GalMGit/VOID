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

    public async Task<FileUploadResult> UploadAvatarAsync(
        UploadFile file,
        Guid userId,
        CancellationToken ct = default)
    {
        if (file.Length > 6 * 1024 * 1024)
            throw new ValidationException("File size exceeds 6 MB.");

        var detectedContentType = Validate(file.Stream);

        if (!detectedContentType.StartsWith("image/"))
            throw new ValidationException("Avatar must be an image.");

        file.Stream.Position = 0;

        using var image = await Image.LoadAsync(
            file.Stream, ct);

        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(200, 200),
            Mode = ResizeMode.Crop
        }));

        await using var output = new MemoryStream();

        await image.SaveAsJpegAsync(
            output,
            new JpegEncoder
            {
                Quality = 50
            }, ct);

        output.Position = 0;

        var key = $"users/{userId}/{Guid.NewGuid()}.jpg";

        const string contentType = "image/jpeg";

        await _publicStorage.UploadAsync(
            key,
            output,
            contentType, ct);

        return new FileUploadResult(
            key,
            null,
            contentType);
    }

    public async Task<FileUploadResult> UploadGroupImageAsync(
        UploadFile file,
        Guid groupId,
        CancellationToken ct = default)
    {
        if (file.Length > 6 * 1024 * 1024)
            throw new ValidationException("File size exceeds 6 MB.");

        var detectedContentType = Validate(file.Stream);

        if (!detectedContentType.StartsWith("image/"))
            throw new ValidationException("Group Image must be an image.");

        file.Stream.Position = 0;

        using var image = await Image.LoadAsync(
            file.Stream, ct);

        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(200, 200),
            Mode = ResizeMode.Crop
        }));

        await using var output = new MemoryStream();

        await image.SaveAsJpegAsync(
            output,
            new JpegEncoder
            {
                Quality = 50
            }, ct);

        output.Position = 0;

        var key = $"groups/{groupId}/{Guid.NewGuid()}.jpg";

        const string contentType = "image/jpeg";

        await _publicStorage.UploadAsync(
            key,
            output,
            contentType, ct);

        return new FileUploadResult(
            key,
            null,
            contentType);
    }

    public async Task<FileUploadResult> UploadMessageMediaAsync(
        UploadFile file,
        Guid chatId,
        CancellationToken ct = default)
    {
        var detectedContentType = Validate(file.Stream);

        var isImage = detectedContentType.StartsWith("image/");
        var isVideo = detectedContentType.StartsWith("video/");
        var isAudio = detectedContentType.StartsWith("audio/");

        var maxSize = isVideo
            ? 50 * 1024 * 1024
            : 6 * 1024 * 1024;

        if (file.Length > maxSize)
        {
            throw new ValidationException(
                $"Maximum size is {maxSize / 1024 / 1024} MB.");
        }

        var mediaKey =
            $"messages/{chatId}/{Guid.NewGuid()}{GetExtension(detectedContentType)}";

        if (isVideo)
        {
            file.Stream.Position = 0;

            await using var memory = new MemoryStream();

            await file.Stream.CopyToAsync(memory, ct);

            memory.Position = 0;

            await _privateStorage.UploadAsync(
                mediaKey,
                memory,
                detectedContentType,
                ct);

            memory.Position = 0;

            await using var thumbnail =
                await CreateVideoThumbnailAsync(
                    memory,
                    ct);

            var thumbnailVideoKey =
                $"messages/{chatId}/{Guid.NewGuid()}_thumb.jpg";

            await _privateStorage.UploadAsync(
                thumbnailVideoKey,
                thumbnail,
                "image/jpeg",
                ct);

            return new FileUploadResult(
                mediaKey,
                thumbnailVideoKey,
                detectedContentType);
        }

        if (isAudio)
        {
            file.Stream.Position = 0;

            await _privateStorage.UploadAsync(
                mediaKey,
                file.Stream,
                detectedContentType,
                ct);

            return new FileUploadResult(
                mediaKey,
                null,
                detectedContentType);
        }

        if (isImage)
        {
            file.Stream.Position = 0;

            await using var imageStream =
                new MemoryStream();

            await file.Stream.CopyToAsync(
                imageStream,
                ct);

            imageStream.Position = 0;

            await _privateStorage.UploadAsync(
                mediaKey,
                imageStream,
                detectedContentType,
                ct);

            string? thumbnailKey = null;

            if (detectedContentType != "image/gif")
            {
                imageStream.Position = 0;

                await using var thumbnail =
                    await CreateThumbnailAsync(
                        imageStream,
                        ct);

                thumbnailKey =
                    $"messages/{chatId}/{Guid.NewGuid()}_thumb.jpg";

                await _privateStorage.UploadAsync(
                    thumbnailKey,
                    thumbnail,
                    "image/jpeg",
                    ct);
            }

            return new FileUploadResult(
                mediaKey,
                thumbnailKey,
                detectedContentType);
        }

        throw new ValidationException(
            $"Unsupported content type: {detectedContentType}");
    }


    public Task DeleteChatMessagesFolderAsync(
        Guid chatId,
        CancellationToken ct = default)
        => _privateStorage.DeletePrefixAsync(
            $"messages/{chatId}/", ct);

    public string GetAvatarUrl(string key)
        => _publicStorage.GetPublicUrl(key);

    public string GetMessageMediaUrl(
        string key,
        TimeSpan lifetime)
        => _privateStorage.GetPresignedUrl(
            key,
            lifetime);

    public async Task DeleteMediaAsync(
        string? mediaPath,
        string? thumbnailPath = null,
        CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(thumbnailPath))
            await _privateStorage.DeleteAsync(
                thumbnailPath,
                ct);

        if (!string.IsNullOrWhiteSpace(mediaPath))
            await _privateStorage.DeleteAsync(
                mediaPath,
                ct);
    }

    public async Task DeleteAvatarAsync(
        string? avatarPath,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(avatarPath))
            return;

        await _publicStorage.DeleteAsync(
            avatarPath,
            ct);
    }

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

    private async Task<MemoryStream> CreateThumbnailAsync(
        Stream stream,
        CancellationToken ct)
    {
        stream.Position = 0;

        using var image = await Image.LoadAsync(stream, ct);

        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(200, 200),
            Mode = ResizeMode.Crop
        }));

        var output = new MemoryStream();

        await image.SaveAsJpegAsync(
            output,
            new JpegEncoder
            {
                Quality = 45
            },
            ct);

        output.Position = 0;

        return output;
    }

    private async Task<MemoryStream> CreateVideoThumbnailAsync(
        Stream videoStream,
        CancellationToken ct)
    {
        var inputFile = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid()}.mp4");

        var outputFile = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid()}.jpg");

        try
        {
            videoStream.Position = 0;

            await using (var file = File.Create(inputFile))
            {
                await videoStream.CopyToAsync(file, ct);
            }

            ct.ThrowIfCancellationRequested();
            var process = FFMpegArguments
                .FromFileInput(inputFile)
                .OutputToFile(
                    outputFile,
                    overwrite: true,
                    options => options
                        .Seek(TimeSpan.FromSeconds(2))
                        .WithFrameOutputCount(1))
                .CancellableThrough(ct);

            await process.ProcessAsynchronously();
            ct.ThrowIfCancellationRequested();

            using var image = await Image.LoadAsync(outputFile, ct);

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(300, 200),
                Mode = ResizeMode.Crop
            }));

            var result = new MemoryStream();

            await image.SaveAsJpegAsync(
                result,
                new JpegEncoder
                {
                    Quality = 80
                },
                ct);

            result.Position = 0;

            return result;
        }
        finally
        {
            if (File.Exists(inputFile))
                File.Delete(inputFile);

            if (File.Exists(outputFile))
                File.Delete(outputFile);
        }
    }

    private string GetExtension(string contentType)
    {
        return contentType switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/gif" => ".gif",
            "video/mp4" => ".mp4",
            "video/webm" => ".webm",
            "audio/wav" => ".wav",

            _ => throw new ValidationException(
                $"Unsupported content type: {contentType}")
        };
    }
}