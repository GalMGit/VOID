using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace VOID.Application.Abstractions.IServices.IMediaServices;

public sealed record FileUploadResult(
    string RelativePath, 
    string? ThumbnailRelativePath,
    string? ContentType);

public sealed class UploadFile : IAsyncDisposable
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required long Length { get; init; }
    public required Stream Stream { get; init; }
    
    public ValueTask DisposeAsync()
    {
        return Stream.DisposeAsync();
    }
}

public interface IFileStorageService
{
    Task<FileUploadResult> UploadAvatarAsync(UploadFile file, Guid userId, CancellationToken ct = default);
    Task<FileUploadResult> UploadMessageMediaAsync(UploadFile file, Guid chatId, CancellationToken ct = default);
    Task DeleteMediaAsync(string? mediaUrl, string? thumbnailUrl = null, CancellationToken ct = default);
    Task DeleteChatMessagesFolderAsync(Guid chatId, CancellationToken ct = default);
    string GetAvatarUrl(string key);
    string GetMessageMediaUrl(
        string key,
        TimeSpan lifetime);

    Task DeleteAvatarAsync(
        string? avatarPath,
        CancellationToken ct = default);

    Task<FileUploadResult> UploadGroupImageAsync(
        UploadFile file,
        Guid groupId,
        CancellationToken ct = default);
}

