using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using VOID.Application.Abstractions.IRepositories.IChatRepositories;
using VOID.Application.UseCases.Users.Events.Profile;
using VOID.Shared.Contracts.SignalRTokens;

namespace VOID.Infrastructure.SignalR.Handlers.Users.Updated;

public sealed class UserUpdatedSignalRHandler(
    IChatRepository chatRepository,
    IHubContext<ChatHub> hub,
    ILogger<UserUpdatedSignalRHandler> logger)
{
    public async Task Handle(UserUpdatedEvent @event)
    {
        var relatedUserIds = await chatRepository.GetRelatedUsersIdsAsync(
            @event.UserId);
        
        await hub.Clients.Users(relatedUserIds.Select(x => x.ToString()))
            .SendAsync(
                SignalRTokens.UserNameUpdated, 
                @event.Name,
                @event.UserId);
        
        logger.LogInformation($"Client {@event.UserId} update name {@event.Name}");
    }
}