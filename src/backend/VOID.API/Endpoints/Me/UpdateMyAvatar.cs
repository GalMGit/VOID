using System.Security.Claims;
using VOID.API.Extensions;
using VOID.Application.Abstractions.IServices.IMediaServices;
using VOID.Application.UseCases.Images.Commands.UpdateAvatar;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using VOID.Shared.Contracts.DTOs.Users.Avatars;
using Wolverine;

namespace VOID.API.Endpoints.Me;

public sealed class UpdateMyAvatar : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("me/avatar", async (
                IFormFile image,
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
                    var result = await command.InvokeAsync<AvatarDto>(
                        new UpdateAvatarCommand(
                            user.GetUserId(),
                            upload), ct);
                    
                    return Results.Ok(result);
                }
            })
            .WithTags(Tags.Me)
            .RequireAuthorization()
            .DisableAntiforgery();
    }
}