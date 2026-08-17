using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace VOID.APP.Services.Interfaces.IImage;

public interface IUserImageService
{
    Task DeleteAvatarAsync(CancellationToken ct = default);
    Task<string?> GetAvatarAsync(CancellationToken ct = default);
    Task UpdateGroupImageAsync(MultipartFormDataContent? content, Guid groupId, CancellationToken ct = default);
    Task<string?> UploadAvatarAsync(MultipartFormDataContent? content, CancellationToken ct = default);
}