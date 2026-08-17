using System.Security.Claims;
using VOID.API.Extensions;
using VOID.Application.UseCases.Groups.Commands.LeaveFromGroup;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using Wolverine;

namespace VOID.API.Endpoints.Groups;

public sealed class LeaveFromGroup : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("groups/{groupId:guid}/members/me", async (
            Guid groupId,
            IMessageBus command,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            await command.InvokeAsync(
                new LeaveFromGroupCommand(
                    groupId, 
                    user.GetUserId()), ct);
            
            return Results.NoContent();
        })
        .WithTags(Tags.Groups)
        .RequireAuthorization();
    }
}
