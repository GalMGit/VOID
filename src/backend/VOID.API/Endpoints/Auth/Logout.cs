using VOID.Application.UseCases.Auth.Commands.Logout;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using VOID.Shared.Contracts.DTOs.Auth.Logout;
using Wolverine;

namespace VOID.API.Endpoints.Auth;

public sealed class Logout : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/logout", async (
            LogoutDto dto,
            IMessageBus command,
            CancellationToken ct) =>
            {
                await command.InvokeAsync(
                    new LogoutUserCommand(
                        dto), ct);
            
            return Results.Ok();
        })
        .WithTags(Tags.Auth)
        .RequireAuthorization();
    }
}
