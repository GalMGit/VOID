using System.Security.Claims;
using VOID.API.Extensions;
using VOID.Application.UseCases.Chats.Commands.Delete;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using Wolverine;

namespace VOID.API.Endpoints.Chats;

public sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("chats/{chatId:guid}", async (
                Guid chatId,
                IMessageBus command,
                ClaimsPrincipal user,
                CancellationToken ct) =>
            {
                await command.InvokeAsync(
                    new DeleteChatCommand(
                        chatId, 
                        user.GetUserId()), ct);

                return Results.NoContent();
            })
            .WithTags(Tags.Chats)
            .RequireAuthorization();
    }
}