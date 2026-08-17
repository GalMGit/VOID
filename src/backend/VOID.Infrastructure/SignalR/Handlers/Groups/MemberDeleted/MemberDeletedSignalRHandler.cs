using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using VOID.Application.UseCases.Groups.Events.MemberDeleted;
using VOID.Shared.Contracts.SignalRTokens;

namespace VOID.Infrastructure.SignalR.Handlers.Groups.MemberDeleted;

public sealed class MemberDeletedSignalRHandler(
    IHubContext<ChatHub> hub,
    ILogger<MemberDeletedSignalRHandler> logger)
{
    public async Task Handle(MemberDeletedEvent @event)
    {
        await hub.Clients.Group(@event.GroupId.ToString())
            .SendAsync(
                SignalRTokens.DeleteGroupMember,
                @event.GroupId, 
                @event.MemberId);
        
        logger.LogInformation($"Client {@event.UserId} delete member {@event.MemberId} from group {@event.GroupId}");
    }
}