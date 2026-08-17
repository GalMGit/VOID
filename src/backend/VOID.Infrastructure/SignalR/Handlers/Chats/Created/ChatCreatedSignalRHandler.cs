using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using VOID.Application.UseCases.Chats.Commands.Create;
using VOID.Application.UseCases.Chats.Events.Created;
using VOID.Shared.Contracts.SignalRTokens;

namespace VOID.Infrastructure.SignalR.Handlers.Chats.Created;

public sealed class ChatCreatedSignalRHandler(
    IHubContext<ChatHub> hub,
    ILogger<ChatCreatedSignalRHandler> logger)
{
    public async Task Handle(ChatCreatedEvent @event)
    {
        await hub.Clients.User(@event.RecipientId.ToString())
            .SendAsync(
                SignalRTokens.PrivateChatCreated,
                @event.RecipientChat);

        await hub.Clients.User(@event.CreatorId.ToString())
            .SendAsync(
                SignalRTokens.PrivateChatCreated,
                @event.CreatorChat);

        logger.LogInformation("User {EventCreatorId} created chat {EventChatId} with user {EventRecipientId}", 
            @event.CreatorId, @event.ChatId, @event.RecipientId);
    }
}