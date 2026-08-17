using System.Security.Claims;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using VOID.API.Extensions;
using VOID.Application.Abstractions.IServices.IMediaServices;
using VOID.Application.UseCases.Images.Commands.UpdateAvatar;
using VOID.Application.UseCases.Images.Commands.UpdateGroupImage;
using VOID.Shared.Contracts.DTOs.Users.Avatars;
using Wolverine;

namespace VOID.API.Endpoints.Groups;

public sealed class UpdateGroupImage : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("groups/{groupId:guid}/image", async (
                IFormFile image,
                Guid groupId,
                IMessageBus command,
                ClaimsPrincipal user,
                CancellationToken ct) =>
            {
                var upload = new UploadFile
                {
                    FileName = image.FileName,
                    ContentType = image.ContentType,
                    Length = image.Length,
                    Stream = image.OpenReadStream()
                };

                await using (upload)
                {
                    await command.InvokeAsync(
                        new UpdateGroupImageCommand(
                            user.GetUserId(),
                            groupId,
                            upload), ct);
                    
                    return Results.Ok();
                }
            })
            .WithTags(Tags.Groups)
            .RequireAuthorization()
            .DisableAntiforgery();
    }
}