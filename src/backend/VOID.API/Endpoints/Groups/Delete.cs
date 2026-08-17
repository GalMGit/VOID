using System.Security.Claims;
using VOID.API.Extensions;
using VOID.Application.UseCases.Groups.Commands.Delete;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using Wolverine;

namespace VOID.API.Endpoints.Groups;

public sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("groups/{groupId:guid}", async (
            Guid groupId,
            IMessageBus command,
            ClaimsPrincipal user,
            CancellationToken ct) =>
            {
                await command.InvokeAsync(
                    new DeleteGroupCommand(
                        groupId,
                        user.GetUserId()), ct);
            
            return Results.NoContent();
        })
        .WithTags(Tags.Groups)
        .RequireAuthorization();
    }
}
