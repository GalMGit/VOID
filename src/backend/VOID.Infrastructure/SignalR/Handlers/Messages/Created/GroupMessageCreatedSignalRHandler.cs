using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using VOID.Application.UseCases.Messages.Events.Created;
using VOID.Shared.Contracts.SignalRTokens;

namespace VOID.Infrastructure.SignalR.Handlers.Messages.Created;

public sealed class GroupMessageCreatedSignalRHandler(
    IHubContext<ChatHub> hub,
    ILogger<GroupMessageCreatedSignalRHandler> logger)
{
    public async Task Handle(GroupMessageCreatedEvent @event)
    {
        await hub.Clients.Group(@event.Message.ParentId.ToString())
            .SendAsync(
                SignalRTokens.NewGroupMessage, 
                @event.Message);
            
        logger.LogInformation($"Client {@event.Message.SenderId} send message to group: {@event.Message.ParentId}");
    }
}
