using System.Security.Claims;
using VOID.API.Extensions;
using VOID.Application.UseCases.Images.Query.GetAvatar;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using VOID.Shared.Contracts.DTOs.Users.Avatars;
using Wolverine;

namespace VOID.API.Endpoints.Me;

public sealed class GetMyAvatar : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("me/avatar", async (
            IMessageBus query,
            ClaimsPrincipal user, 
            CancellationToken ct) =>
            {
                var result = await query.InvokeAsync<AvatarDto>(
                    new GetAvatarQuery(
                        user.GetUserId()), ct);
                
                return Results.Ok(result);
            })
        .WithTags(Tags.Me)
        .RequireAuthorization();
    }
}