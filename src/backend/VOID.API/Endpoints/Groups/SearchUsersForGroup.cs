using System.Security.Claims;
using VOID.API.Extensions;
using VOID.Application.UseCases.Groups.Queries.SearchUsersForGroup;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using VOID.Shared.Contracts.DTOs.Users;
using Wolverine;

namespace VOID.API.Endpoints.Groups;

public sealed class SearchUsersForGroup : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("groups/{groupId:guid}/users/search/{term}", async (
            Guid groupId,
            string term,
            ClaimsPrincipal user,
            IMessageBus query, 
            CancellationToken ct) =>
        {
            var result = await query.InvokeAsync<List<SearchUserDto>>(
                    new SearchUsersForGroupQuery(
                        term, 
                        user.GetUserId(), 
                        groupId), ct);
            
            return Results.Ok(result);
        })
        .WithTags(Tags.Groups)
        .RequireAuthorization();
    }
}