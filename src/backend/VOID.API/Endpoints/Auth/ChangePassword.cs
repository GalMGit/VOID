using System.Security.Claims;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using VOID.API.Extensions;
using VOID.API.Filters;
using VOID.Application.UseCases.Auth.Commands.ChangePassword;
using VOID.Shared.Contracts.DTOs.Auth.ChangePassword;
using Wolverine;

namespace VOID.API.Endpoints.Auth;

public sealed class ChangePassword : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("auth/change-password", async (
                ChangePasswordDto dto,
                IMessageBus command,
                ClaimsPrincipal user,
                CancellationToken ct) =>
            {
                await command.InvokeAsync(
                    new ChangePasswordCommand(
                        dto, 
                        user.GetUserId()), ct);
                
                return Results.Ok("Пароль изменен");
            })
            .RequireAuthorization()
            .WithTags(Tags.Auth)
            .AddEndpointFilter<FluentValidationFilter<ChangePasswordDto>>();
    }
}