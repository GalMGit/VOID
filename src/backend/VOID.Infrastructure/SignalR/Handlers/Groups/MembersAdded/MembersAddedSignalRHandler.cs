using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using VOID.Application.UseCases.Groups.Events.MembersAdded;
using VOID.Shared.Contracts.SignalRTokens;

namespace VOID.Infrastructure.SignalR.Handlers.Groups.MembersAdded;

public sealed class MembersAddedSignalRHandler(
    IHubContext<ChatHub> hub,
    ILogger<MembersAddedSignalRHandler> logger)
{
    public async Task Handle(MembersAddedEvent @event)
    {
        List<string> membersToString = [];
        membersToString.AddRange(@event.MembersIds
            .Select(memberId => memberId.ToString()));
        
        await hub.Clients.Users(membersToString)
            .SendAsync(
                SignalRTokens.AddedToGroup, 
                @event.Group, 
                @event.SenderId);
        
        logger.LogInformation($"Client {@event.SenderId} added users to {@event.Group.Id}");
    }
}