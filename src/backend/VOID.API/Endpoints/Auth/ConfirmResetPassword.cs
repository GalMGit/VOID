using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using VOID.API.Filters;
using VOID.Application.UseCases.Auth.Commands.ConfirmResetPassword;
using VOID.Shared.Contracts.DTOs.Auth.ConfirmResetPassword;
using Wolverine;

namespace VOID.API.Endpoints.Auth;

public sealed class ConfirmResetPassword : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/confirm-password", async (
                ConfirmResetPasswordDto dto,
                IMessageBus command,
                CancellationToken ct) =>
            {
                var result = await command.InvokeAsync<string>(
                    new ConfirmResetPasswordCommand(
                        dto), ct);

                return Results.Ok(result);
            })
            .WithTags(Tags.Auth)
            .AddEndpointFilter<FluentValidationFilter<ConfirmResetPasswordDto>>();
    }
}