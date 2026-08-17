using Microsoft.AspNetCore.SignalR;
using VOID.Application.UseCases.Groups.Events.Created;
using VOID.Shared.Contracts.SignalRTokens;

namespace VOID.Infrastructure.SignalR.Handlers.Groups.Created;

public sealed class GroupCreatedSignalRHandler(IHubContext<ChatHub> hub)
{
    public async Task Handle(GroupCreatedEvent @event)
    {
        await hub.Clients.User(@event.UserId.ToString())
            .SendAsync(
                SignalRTokens.GroupCreated, 
                @event.Group);
    }
}