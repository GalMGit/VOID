using System.Threading;
using System.Threading.Tasks;
using VOID.Application.Abstractions.IRepositories.IImageRepositories;
using VOID.Application.Abstractions.IServices.IMediaServices;
using VOID.Shared.Contracts.DTOs.Users.Avatars;

namespace VOID.Application.UseCases.Images.Query.GetAvatar;

public sealed class GetAvatarQueryHandler(
    IImageRepository imageRepository,
    IMediaUrlService mediaUrlService)
{
    public async Task<AvatarDto> HandleAsync(
        GetAvatarQuery request, 
        CancellationToken ct)
    {
        var avatarPath = await imageRepository.GetAvatarUrlByUserAsync(
            request.UserId, ct);

        return new AvatarDto
        {
            AvatarUrl = mediaUrlService.GetAvatarUrl(avatarPath)
        };
    }
}