using Microsoft.AspNetCore.SignalR;
using VOID.Application.UseCases.Groups.Events.Deleted;
using VOID.Shared.Contracts.SignalRTokens;

namespace VOID.Infrastructure.SignalR.Handlers.Groups.Deleted;

public sealed class GroupDeletedSignalRHandler(
    IHubContext<ChatHub> hub)
{
    public async Task Handle(GroupDeletedEvent @event)
    {
        await hub.Clients.Group(@event.GroupId.ToString())
            .SendAsync(
                SignalRTokens.GroupDeleted, 
                @event.GroupId);
    }
}