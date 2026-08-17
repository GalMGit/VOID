using Microsoft.AspNetCore.SignalR;
using VOID.Application.UseCases.Messages.Events.Cleared;
using VOID.Shared.Contracts.SignalRTokens;

namespace VOID.Infrastructure.SignalR.Handlers.Messages.Cleared;

public sealed class MessagesByChatClearedSignalRHandler(
    IHubContext<ChatHub> hub)
{
    public async Task Handle(MessagesByChatClearedEvent @event)
    {
        await hub.Clients.Users([
                @event.RecipientId.ToString(),
                @event.UserId.ToString()])
            .SendAsync(
                SignalRTokens.ChatCleared,
                @event.ChatId);
    }
}