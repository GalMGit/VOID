using VOID.Application.UseCases.Auth.Commands.Login;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using VOID.API.Filters;
using VOID.Shared.Contracts.DTOs.Auth.Login;
using Wolverine;

namespace VOID.API.Endpoints.Auth;

public sealed class Login : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/login", async (
            LoginUserDto dto,
            IMessageBus command,
            CancellationToken ct) =>
        {
            var result = await command.InvokeAsync<LoginDto>(
                    new LoginUserCommand(
                        dto), ct);

            return Results.Ok(result);
        })
        .WithTags(Tags.Auth)
        .AddEndpointFilter<FluentValidationFilter<LoginUserDto>>();
    }
}