using System.Security.Claims;
using VOID.API.Extensions;
using VOID.Application.UseCases.Groups.Commands.AddMembers;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using VOID.Shared.Contracts.DTOs.Groups;
using Wolverine;

namespace VOID.API.Endpoints.Groups;

public sealed class AddMembers : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("groups/{groupId:guid}", async (
            Guid groupId,
            AddGroupMembersDto dto,
            IMessageBus command,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var result = await command.InvokeAsync<List<GroupMemberDto>>(
                    new AddMembersToGroupCommand(
                        dto, 
                        groupId, 
                        user.GetUserId()), ct);
            
            return Results.Ok(result);
        })
        .WithTags(Tags.Groups)
        .RequireAuthorization();
    }
}
