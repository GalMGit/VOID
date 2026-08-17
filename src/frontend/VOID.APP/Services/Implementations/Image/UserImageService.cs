using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using VOID.APP.Services.Interfaces.IImage;
using VOID.Shared.Contracts.DTOs.Users.Avatars;

namespace VOID.APP.Services.Implementations.Image;

public class UserImageService(
    HttpClient httpClient
) : IUserImageService
{
    public async Task DeleteAvatarAsync(CancellationToken ct = default)
        => await httpClient.DeleteAsync("me/avatar", ct);

    public async Task<string?> GetAvatarAsync(CancellationToken ct = default)
    {
        var response = await httpClient.GetAsync(
            "me/avatar", ct);
        
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content
                .ReadFromJsonAsync<AvatarDto>(ct);
            
            if (!string.IsNullOrEmpty(result?.AvatarUrl))
                return result.AvatarUrl;
        }

        return null;
    }

    public async Task UpdateGroupImageAsync(
        MultipartFormDataContent? content,
        Guid groupId,
        CancellationToken ct = default)
        => await httpClient.PatchAsync(
            $"groups/{groupId}/image",
            content, ct);
    
    public async Task<string?> UploadAvatarAsync(
        MultipartFormDataContent? content, 
        CancellationToken ct = default)
    {
        var response = await httpClient.PatchAsync(
            "me/avatar",
            content, ct);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content
                .ReadFromJsonAsync<AvatarDto>(ct);
            
            var avatarUrl = result?.AvatarUrl;
            
            return avatarUrl;
        }

        return string.Empty;
    }
}