using System.Security.Claims;
using VOID.API.Extensions;
using VOID.Application.UseCases.Messages.Commands.ClearByChat;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using Wolverine;

namespace VOID.API.Endpoints.Chats;

public sealed class ClearChatMessages : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("chats/{chatId:guid}/messages", async (
            Guid chatId,
            IMessageBus command,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            await command.InvokeAsync(
                new ClearByChatCommand(
                    chatId,
                    user.GetUserId()), ct);

            return Results.NoContent();
        })
        .WithTags(Tags.Chats)
        .RequireAuthorization();
    }
}