using System.Security.Claims;
using VOID.API.Extensions;
using VOID.Application.UseCases.Images.Commands.UpdateAvatar;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using Wolverine;

namespace VOID.API.Endpoints.Me;

public sealed class DeleteMyAvatar : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("me/avatar", async (
                IMessageBus command,
                ClaimsPrincipal user, 
                CancellationToken ct) =>
            {
                await command.InvokeAsync(
                    new UpdateAvatarCommand(
                        user.GetUserId(),
                        null), ct);
                
                return Results.NoContent();
            })
            .WithTags(Tags.Me)
            .RequireAuthorization();
    }
}
