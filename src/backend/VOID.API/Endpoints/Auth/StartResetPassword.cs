using System.Security.Claims;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using VOID.API.Filters;
using VOID.Application.UseCases.Auth.Commands.ResetPassword;
using VOID.Shared.Contracts.DTOs.Auth.ResetPassword;
using Wolverine;

namespace VOID.API.Endpoints.Auth;

public sealed class StartResetPassword : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/reset-password", async (
                StartResetPasswordDto dto,
                IMessageBus command,
                CancellationToken ct) =>
            {
                await command.InvokeAsync(
                    new StartResetPasswordCommand(dto), ct);
            })
            .WithTags(Tags.Auth)
            .AddEndpointFilter<FluentValidationFilter<StartResetPasswordDto>>();
    }
}