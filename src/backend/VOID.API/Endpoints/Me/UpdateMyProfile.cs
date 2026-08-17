using System.Security.Claims;
using VOID.API.Extensions;
using VOID.Application.UseCases.Users.Commands.Update;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using VOID.Shared.Contracts.DTOs.Users.Accounts;
using Wolverine;

namespace VOID.API.Endpoints.Me;

public sealed class UpdateMyProfile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("me/profile", async (
            UpdateUserDto dto,
            IMessageBus command,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var result = await command.InvokeAsync<UserAuthDto>(
                    new UpdateUserCommand(
                        dto, 
                        user.GetUserId()), ct);
               
            return Results.Ok(result);
        })
        .WithTags(Tags.Me)
        .RequireAuthorization();
    }
}
