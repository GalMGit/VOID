using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using VOID.Application.UseCases.Images.Events;
using VOID.Shared.Contracts.SignalRTokens;

namespace VOID.Infrastructure.SignalR.Handlers.Groups.ImageUpdated;

public sealed class GroupImageUpdatedSignalRHandler(
    IHubContext<ChatHub> hub,
    ILogger<GroupImageUpdatedSignalRHandler> logger)
{
    public async Task Handle(GroupImageUpdatedEvent @event)
    {
        await hub.Clients.Group(@event.GroupId.ToString())
            .SendAsync(
                SignalRTokens.GroupImageUpdated, 
                @event.GroupId,
                @event.ImageUrl);
        
        logger.LogInformation($"User {@event.UserId} update group image {@event.ImageUrl}");
    }
}