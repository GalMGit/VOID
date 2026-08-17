using System.Security.Claims;
using VOID.API.Extensions;
using VOID.Application.UseCases.Chats.Queries.GetById;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using VOID.Shared.Contracts.DTOs.Chats;
using Wolverine;

namespace VOID.API.Endpoints.Chats;

public sealed class GetChatById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("chats/{chatId:guid}", async (
            Guid chatId,
            IMessageBus query,
            ClaimsPrincipal user, 
            CancellationToken ct) =>
        {
            var result = await query.InvokeAsync<FullChatDto>(
                    new GetChatByIdQuery(
                        chatId, 
                        user.GetUserId()), ct);
                
            return Results.Ok(result);
        })
        .WithTags(Tags.Chats)
        .RequireAuthorization();
    }
}
