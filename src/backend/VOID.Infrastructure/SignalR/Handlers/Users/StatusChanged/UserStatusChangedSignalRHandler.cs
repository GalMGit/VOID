using Microsoft.AspNetCore.SignalR;
using VOID.Application.Abstractions.IRepositories.IChatRepositories;
using VOID.Application.UseCases.Users.Events.Connections;
using VOID.Shared.Contracts.SignalRTokens;

namespace VOID.Infrastructure.SignalR.Handlers.Users.StatusChanged;

public sealed class UserStatusChangedSignalRHandler(
    IHubContext<ChatHub> hub,
    IChatRepository chatRepository)
{
    public async Task Handle(UserStatusChangedEvent @event)
    {
        var relatedUsersIds = await chatRepository.GetRelatedUsersIdsAsync(
            @event.UserId);
        
        await hub.Clients.Users(relatedUsersIds.Select(x => x.ToString()))
            .SendAsync(
                SignalRTokens.UserStatusChanged, 
                @event.UserId, 
                @event.Status);
    }
}