using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using VOID.Application.Abstractions.IRepositories.IImageRepositories;
using VOID.Application.Abstractions.IServices.IMediaServices;
using VOID.Application.UseCases.Images.Events;
using Wolverine;

namespace VOID.Application.UseCases.Images.Commands.UpdateGroupImage;

public sealed class UpdateGroupImageCommandHandler(
    IImageRepository imageRepository,
    IFileStorageService storageService,
    IMapper mapper,
    IMediaUrlService mediaUrlService,
    IMessageBus bus)
{
    public async Task Handle(
        UpdateGroupImageCommand request, 
        CancellationToken ct)
    {
        FileUploadResult? image = null;
        
        if (request.Media is not null)
            image = await storageService.UploadGroupImageAsync(
                request.Media!, 
                request.GroupId, ct);
        
        await imageRepository.UpdateGroupImageAsync(
            image?.RelativePath, 
            request.GroupId,
            ct);

        var avatarUrl = mediaUrlService.GetAvatarUrl(image?.RelativePath);

        await bus.PublishAsync(
            new GroupImageUpdatedEvent(
                request.UserId,
                request.GroupId, 
                avatarUrl));
    }
}