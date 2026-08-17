using VOID.Application.UseCases.Users.Queries.GetUserInfo;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using VOID.Shared.Contracts.DTOs.Users.Accounts;
using Wolverine;

namespace VOID.API.Endpoints.Users;

public sealed class GetAccountInfo : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/account/{userId:guid}/info", async (
            Guid userId,
            IMessageBus query,
            CancellationToken ct) =>
            {
                var result = await query.InvokeAsync<UserAuthDto>(
                    new GetUserInfoQuery(
                        userId), ct);
                
            return Results.Ok(result);
        })
        .WithTags(Tags.Users)
        .RequireAuthorization();
    }
}
