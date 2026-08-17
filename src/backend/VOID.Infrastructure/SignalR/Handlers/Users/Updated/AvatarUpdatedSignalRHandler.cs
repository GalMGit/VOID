using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using VOID.Application.Abstractions.IRepositories.IChatRepositories;
using VOID.Application.UseCases.Images.Events;
using VOID.Shared.Contracts.SignalRTokens;

namespace VOID.Infrastructure.SignalR.Handlers.Users.Updated;

public sealed class AvatarUpdatedSignalRHandler(
    IChatRepository chatRepository,
    IHubContext<ChatHub> hub,
    ILogger<AvatarUpdatedSignalRHandler> logger)
{
    public async Task Handle(AvatarUpdatedEvent @event)
    {
        var relatedUsersIds = await chatRepository.GetRelatedUsersIdsAsync(
            @event.UserId);
        
        await hub.Clients.Users(relatedUsersIds.Select(x => x.ToString()))
            .SendAsync(
                SignalRTokens.AvatarUpdated, 
                @event.UserId,
                @event.AvatarUrl);
        
        logger.LogInformation($"User {@event.UserId} updated avatar {@event.AvatarUrl}");
    }
}