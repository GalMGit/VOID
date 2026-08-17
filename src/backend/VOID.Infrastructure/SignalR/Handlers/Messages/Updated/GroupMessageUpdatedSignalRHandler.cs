using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using VOID.Application.UseCases.Messages.Events.Updated;
using VOID.Shared.Contracts.SignalRTokens;

namespace VOID.Infrastructure.SignalR.Handlers.Messages.Updated;

public sealed class GroupMessageUpdatedSignalRHandler(
    IHubContext<ChatHub> hub,
    ILogger<GroupMessageUpdatedSignalRHandler> logger)
{
    public async Task Handle(GroupMessageUpdatedEvent @event)
    {
        await hub.Clients.Group(@event.GroupId.ToString())
            .SendAsync(
                SignalRTokens.MessageInGroupUpdated, 
                @event.Message, 
                @event.GroupId);
        
        logger.LogInformation($"Client {@event.UserId} update message {@event.Message.Id} in group {@event.GroupId}");
    }
}