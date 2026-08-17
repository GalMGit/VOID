using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using VOID.Application.UseCases.Messages.Events.Deleted;
using VOID.Shared.Contracts.SignalRTokens;

namespace VOID.Infrastructure.SignalR.Handlers.Messages.Deleted;

public sealed class GroupMessageDeletedSignalRHandler(
    IHubContext<ChatHub> hub,
    ILogger<GroupMessageDeletedSignalRHandler> logger)
{
    public async Task Handle(GroupMessageDeletedEvent @event)
    {
        await hub.Clients.Group(@event.GroupId.ToString())
            .SendAsync(
                SignalRTokens.GroupMessageDeleted, 
                @event.GroupId, 
                @event.MessageId);
        
        logger.LogInformation($"Client {@event.UserId} delete message {@event.MessageId} in group {@event.GroupId}");
    }
}