using System.Threading;
using System.Threading.Tasks;
using VOID.Application.Extensions;
using VOID.Application.Abstractions.IRepositories.IMessageRepositories;
using VOID.Shared.Contracts.Enums.Chats;
using Wolverine;

namespace VOID.Application.UseCases.Messages.Events.MarkRead;

public sealed class MarkMessagesReadEventHandler(
    IMessageRepository messageRepository,
    IMessageBus bus)
{
    public async Task Handle(
        MarkMessagesReadEvent @event)
    {
        await messageRepository.ReadMessagesAsync(
            @event.ChatId, 
            @event.UserId,
            @event.ChatType.ToDomain());
        
        if (@event.RecipientId is not null 
            && @event.ChatType == ChatType.Private)
        {
            await bus.PublishAsync(
                new PrivateMessagesReadEvent(
                    @event.RecipientId.Value,
                    @event.ChatId));
        }
        else
        {
            await bus.PublishAsync(
                new GroupMessagesReadEvent(
                    @event.ChatId));
        }
    }
}