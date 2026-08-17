using Microsoft.AspNetCore.SignalR;
using VOID.Application.UseCases.Messages.Events.MarkRead;
using VOID.Shared.Contracts.SignalRTokens;

namespace VOID.Infrastructure.SignalR.Handlers.Messages.Readed;

public sealed class GroupMessagesReadSignalRHandler(
    IHubContext<ChatHub> hub)
{
    public async Task Handle(GroupMessagesReadEvent @event)
    {
        await hub.Clients.Group(@event.GroupId.ToString())
            .SendAsync(
                SignalRTokens.GroupMessagesRead, 
                @event.GroupId);
    }
}