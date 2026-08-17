using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using VOID.Application.Abstractions.IRepositories.IMessageRepositories;
using VOID.Application.Abstractions.IServices.ISecurityServices;
using VOID.Application.UseCases.Messages.Events.Deleted;
using VOID.Shared.Contracts.SignalRTokens;

namespace VOID.Infrastructure.SignalR.Handlers.Messages.Deleted;

public sealed class PrivateMessageDeletedSignalRHandler(
    IHubContext<ChatHub> hub,
    IMessageRepository repository,
    IMapper mapper,
    IEncryptionService encryptionService,
    ILogger<PrivateMessageDeletedSignalRHandler> logger)
{
    public async Task Handle(PrivateMessageDeletedEvent @event)
    {
        await hub.Clients.Users([
                @event.RecipientId.ToString(), 
                @event.UserId.ToString()
            ])
            .SendAsync(
                SignalRTokens.MessageDeleted, 
                @event.ChatId, 
                @event.MessageId,
                @event.LastMessage);
    }
}