using System.Threading;
using System.Threading.Tasks;
using VOID.Application.Abstractions.IRepositories.IImageRepositories;
using VOID.Application.Abstractions.IServices.IMediaServices;
using VOID.Application.UseCases.Images.Events;
using VOID.Shared.Contracts.DTOs.Users.Avatars;
using Wolverine;

namespace VOID.Application.UseCases.Images.Commands.UpdateAvatar;

public sealed class UpdateAvatarCommandHandler(
    IImageRepository imageRepository, 
    IFileStorageService storageService,
    IMediaUrlService mediaUrlService,
    IMessageBus bus) 
{
    public async Task<AvatarDto> Handle(
        UpdateAvatarCommand request, 
        CancellationToken ct)
    {
        FileUploadResult? avatar = null;
        
        var oldImageUrl = await imageRepository.GetAvatarUrlByUserAsync(
            request.UserId, ct);
        
        if (oldImageUrl is not null)
        {
            await storageService.DeleteAvatarAsync(
                oldImageUrl, ct);
        }
        
        if (request.Media is not null)
            avatar = await storageService.UploadAvatarAsync(
                request.Media!, 
                request.UserId, ct);
        
        await imageRepository.UpdateAvatarAsync(
            avatar?.RelativePath, 
            request.UserId,
            ct);

        var avatarDto = new AvatarDto
        {
            AvatarUrl = mediaUrlService.GetAvatarUrl(avatar?.RelativePath)
        };

        await bus.PublishAsync(new AvatarUpdatedEvent(
            request.UserId,
            avatarDto.AvatarUrl));

        return avatarDto;
    }
}