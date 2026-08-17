using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using VOID.Application.UseCases.Groups.Events.Leaved;
using VOID.Shared.Contracts.SignalRTokens;

namespace VOID.Infrastructure.SignalR.Handlers.Groups.Leaved;

public sealed class LeavedFromGroupSignalRHandler(
    IHubContext<ChatHub> hub,
    ILogger<LeavedFromGroupSignalRHandler> logger)
{
    public async Task Handle(LeavedFromGroupEvent @event)
    {
        await hub.Clients.Group(@event.GroupId.ToString())
            .SendAsync(
                SignalRTokens.UserLeaveFromGroup, 
                @event.UserId,
                @event.GroupId);
        
        logger.LogInformation($"Client {@event.UserId} leave from group {@event.GroupId}");
    }
}