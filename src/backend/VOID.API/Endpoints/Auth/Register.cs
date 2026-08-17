using VOID.Application.UseCases.Auth.Commands.Register;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using VOID.API.Filters;
using VOID.Shared.Contracts.DTOs.Auth.Register;
using Wolverine;

namespace VOID.API.Endpoints.Auth;

public sealed class Register : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/register", async (
            RegisterUserDto dto,
            IMessageBus command,
            CancellationToken ct) =>
        {
            var result = await command.InvokeAsync<RegisterDto>(
                new RegisterUserCommand(
                    dto), ct);
            
            return Results.Ok(result);
        })
        .WithTags(Tags.Auth)
        .AddEndpointFilter<FluentValidationFilter<RegisterUserDto>>();
    }
}
