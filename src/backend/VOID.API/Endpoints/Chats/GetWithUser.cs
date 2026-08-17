using System.Security.Claims;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using VOID.API.Extensions;
using VOID.Application.UseCases.Chats.Queries.GetWithUser;
using VOID.Shared.Contracts.DTOs.Chats;
using Wolverine;

namespace VOID.API.Endpoints.Chats;

public sealed class GetWithUser : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("chats/private/{userId:guid}", async (
            Guid userId,
            IMessageBus query,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var result = await query.InvokeAsync<ChatDto?>(
                new GetPrivateChatQuery(
                    user.GetUserId(),
                    userId), ct);

            return Results.Ok(result);
            
        })
        .WithTags(Tags.Chats)
        .RequireAuthorization();
    }
}