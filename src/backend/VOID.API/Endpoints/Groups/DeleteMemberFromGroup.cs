using System.Security.Claims;
using VOID.API.Extensions;
using VOID.Application.UseCases.Groups.Commands.DeleteMembers;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using Wolverine;

namespace VOID.API.Endpoints.Groups;

public sealed class DeleteMemberFromGroup : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("groups/{groupId:guid}/members/{memberId:guid}", async (
            Guid groupId,
            Guid memberId,
            IMessageBus command,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            await command.InvokeAsync(
                    new DeleteMemberFromGroupCommand(
                        groupId, 
                        memberId, 
                        user.GetUserId()), ct);
            
            return Results.NoContent();
        })
        .WithTags(Tags.Groups)
        .RequireAuthorization();
    }
}
