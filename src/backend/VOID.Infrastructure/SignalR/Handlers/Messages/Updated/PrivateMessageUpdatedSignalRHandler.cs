using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using VOID.Application.UseCases.Messages.Events.Updated;
using VOID.Shared.Contracts.SignalRTokens;

namespace VOID.Infrastructure.SignalR.Handlers.Messages.Updated;

public sealed class PrivateMessageUpdatedSignalRHandler(
    IHubContext<ChatHub> hub,
    ILogger<PrivateMessageUpdatedSignalRHandler> logger)
{
    public async Task Handle(PrivateMessageUpdatedEvent @event)
    {
        await hub.Clients.Users([
                @event.RecipientId.ToString(),
                @event.UserId.ToString()])
            .SendAsync(
                SignalRTokens.MessageUpdated, 
                @event.Message, 
                @event.ChatId);
        
        logger.LogInformation($"Client {@event.UserId} update message {@event.Message.Id} in chat {@event.ChatId}");
    }
}