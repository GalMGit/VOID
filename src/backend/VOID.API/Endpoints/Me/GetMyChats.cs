using System.Security.Claims;
using VOID.API.Extensions;
using VOID.Application.UseCases.Chats.Queries.GetChatsByUser;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using VOID.Shared.Contracts.DTOs.Chats;
using VOID.Shared.Contracts.DTOs.Paginations;
using Wolverine;

namespace VOID.API.Endpoints.Me;

public sealed class GetMyChats : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("me/chats", async (
            IMessageBus query,
            ClaimsPrincipal user,
            [AsParameters] PaginationRequest pagination,
            CancellationToken ct) =>
        {
            var result = await query.InvokeAsync<PaginatedResult<ChatDto>>(
                    new GetChatsByUserQuery(
                        user.GetUserId(), 
                        pagination), ct);

            return Results.Ok(result);
        })
        .WithTags(Tags.Chats)
        .RequireAuthorization();
    }
}
