using System.Security.Claims;
using VOID.API.Extensions;
using VOID.Application.UseCases.Groups.Queries.GetGroupsByUser;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using VOID.Shared.Contracts.DTOs.Groups;
using VOID.Shared.Contracts.DTOs.Paginations;
using Wolverine;

namespace VOID.API.Endpoints.Me;

public sealed class GetMyGroups : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("me/groups", async (
                IMessageBus query,
                [AsParameters] PaginationRequest pagination,
                ClaimsPrincipal user, 
                CancellationToken ct) =>
            {
                var result = await query.InvokeAsync<PaginatedResult<GroupDto>>(
                        new GetGroupsByUserQuery(
                            user.GetUserId(), 
                            pagination), ct);
                
                return Results.Ok(result);
            })
            .WithTags(Tags.Groups)
            .RequireAuthorization();
    }
}