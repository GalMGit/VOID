using System.Security.Claims;
using VOID.API.Extensions;
using VOID.Application.UseCases.Groups.Commands.Create;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using VOID.API.Filters;
using VOID.Shared.Contracts.DTOs.Groups;
using Wolverine;

namespace VOID.API.Endpoints.Groups;

public sealed class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("groups", async (
            CreateGroupDto dto,
            IMessageBus command,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var result = await command.InvokeAsync<GroupDto>(
                new CreateGroupCommand(
                    dto, 
                    user.GetUserId()), ct);

            return Results.Ok(result);
        })
        .WithTags(Tags.Groups)
        .RequireAuthorization()
        .AddEndpointFilter<FluentValidationFilter<CreateGroupDto>>();
    }
}
