using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using VOID.API.Filters;
using VOID.Application.UseCases.Auth.Commands.CompleteResetPassword;
using VOID.Shared.Contracts.DTOs.Auth.ResetPassword;
using Wolverine;

namespace VOID.API.Endpoints.Auth;

public sealed class CompleteResetPassword : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("auth/reset-password", async (
                CompleteResetPasswordDto dto,
                IMessageBus command,
                CancellationToken ct) =>
            {
                await command.InvokeAsync(
                    new CompleteResetPasswordCommand(
                        dto), ct);
            })
            .WithTags(Tags.Auth)
            .AddEndpointFilter<FluentValidationFilter<CompleteResetPasswordDto>>();
    }
}