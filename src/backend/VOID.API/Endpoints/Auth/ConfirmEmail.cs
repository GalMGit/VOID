using VOID.Application.UseCases.Auth.Commands.ConfirmEmail;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using VOID.Shared.Contracts.DTOs.Auth.Register;
using Wolverine;

namespace VOID.API.Endpoints.Auth;

public sealed class ConfirmEmail : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/email-confirm", async (
            ConfirmEmailDto dto,
            IMessageBus command,
            CancellationToken ct) =>
        {
            await command.InvokeAsync(
                new ConfirmEmailCommand(
                    dto), ct);
            
            return Results.Ok();
        })
        .WithTags(Tags.Auth);
    }
}