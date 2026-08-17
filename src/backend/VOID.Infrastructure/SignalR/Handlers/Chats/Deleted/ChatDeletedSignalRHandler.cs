using Microsoft.AspNetCore.SignalR;
using VOID.Application.UseCases.Chats.Events.Deleted;
using VOID.Shared.Contracts.SignalRTokens;

namespace VOID.Infrastructure.SignalR.Handlers.Chats.Deleted;

public sealed class ChatDeletedSignalRHandler(
    IHubContext<ChatHub> hub)
{
    public async Task Handle(ChatDeletedEvent @event)
    {
        await hub.Clients.Users([
                @event.RecipientId.ToString(), 
                @event.UserId.ToString()
            ])
            .SendAsync(
                SignalRTokens.ChatDeleted, 
                @event.ChatId);
    }
}