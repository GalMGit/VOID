using Microsoft.AspNetCore.SignalR;
using VOID.Application.UseCases.Messages.Events.MarkRead;
using VOID.Shared.Contracts.SignalRTokens;

namespace VOID.Infrastructure.SignalR.Handlers.Messages.Readed;

public sealed class PrivateMessagesReadSignalRHandler(
    IHubContext<ChatHub> hub)
{
    public async Task Handle(PrivateMessagesReadEvent @event)
    {
        await hub.Clients.User(@event.RecipientId.ToString())
            .SendAsync(
                SignalRTokens.MessagesRead, 
                @event.ChatId);
    }
}