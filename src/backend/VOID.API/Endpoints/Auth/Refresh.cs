using VOID.Application.UseCases.Auth.Commands.Refresh;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using VOID.Shared.Contracts.DTOs.Auth.Login;
using VOID.Shared.Contracts.DTOs.Auth.Token;
using Wolverine;

namespace VOID.API.Endpoints.Auth;

public sealed class Refresh : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/refresh", async (
            RefreshTokenDto dto,
            IMessageBus command,
            CancellationToken ct) =>
        {
            var result = await command.InvokeAsync<LoginDto>(
                    new RefreshTokenCommand(
                        dto), ct);
            
            return Results.Ok(result);
        })
        .WithTags(Tags.Auth);
    }
}
