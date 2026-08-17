using System.Security.Claims;
using VOID.API.Extensions;
using VOID.Application.UseCases.Groups.Queries.GetById;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using VOID.Shared.Contracts.DTOs.Groups;
using Wolverine;

namespace VOID.API.Endpoints.Groups;

public sealed class GetGroupById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("groups/{groupId}", async (
                Guid groupId,
                IMessageBus query,
                ClaimsPrincipal user, 
                CancellationToken ct) =>
            {
                var result = await query.InvokeAsync<FullGroupDto>(
                    new GetGroupByIdQuery(
                        user.GetUserId(), 
                        groupId), ct);
                
                return Results.Ok(result);
            })
            .WithTags(Tags.Groups)
            .RequireAuthorization();
    }
}