using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using VOID.Application.UseCases.Messages.Events.Created;
using VOID.Shared.Contracts.SignalRTokens;

namespace VOID.Infrastructure.SignalR.Handlers.Messages.Created;

public sealed class PrivateMessageCreatedSignalRHandler(
    IHubContext<ChatHub> hub,
    ILogger<PrivateMessageCreatedSignalRHandler> logger)
{
    public async Task Handle(PrivateMessageCreatedEvent @event)
    {
        await hub.Clients.Users([
                @event.RecipientId.ToString(), 
                @event.Message.SenderId.ToString()
            ])
            .SendAsync(
                SignalRTokens.NewMessage, 
                @event.Message);
        
        logger.LogInformation($"Client {@event.Message.SenderId} send message to chat {@event.Message.ParentId}");
    }
}