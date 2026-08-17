using System.Security.Claims;
using VOID.API.Extensions;
using VOID.Application.UseCases.Users.Queries.SearchUsers;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using VOID.Shared.Contracts.DTOs.Users;
using Wolverine;

namespace VOID.API.Endpoints.Users;

public sealed class SearchUsers : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/search/{term}", async (
            string term,
            IMessageBus query,
            ClaimsPrincipal user, 
            CancellationToken ct) =>
        {
            var result = await query.InvokeAsync<List<SearchUserDto>>(
                new SearchUsersQuery(
                    term,
                    user.GetUserId()), ct);

            return Results.Ok(result);
        })
        .WithTags(Tags.Users)
        .RequireAuthorization();
    }
}