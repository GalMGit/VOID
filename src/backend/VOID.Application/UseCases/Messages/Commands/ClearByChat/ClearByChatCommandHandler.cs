using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VOID.Application.Abstractions.IRepositories.IChatRepositories;
using VOID.Application.Abstractions.IRepositories.IMessageRepositories;
using VOID.Application.Abstractions.IServices.IMediaServices;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Messages.Events.Cleared;
using Wolverine;

namespace VOID.Application.UseCases.Messages.Commands.ClearByChat;

public sealed class ClearByChatCommandHandler(
    IMessageRepository messageRepository,
    IFileStorageService storageService,
    IChatRepository chatRepository,
    IMessageBus bus)
{
    public async Task Handle(
        ClearByChatCommand request, 
        CancellationToken ct)
    {
        var chat = await chatRepository.GetByIdAsync(
                       request.ChatId, ct)
                   ?? throw new NotFoundException("Чат не найден");

        if (chat.Interlocutors
            .All(x => x.UserId != request.UserId))
            throw new ForbiddenException();

        var recipientId = chat.Interlocutors
            .First(x => x.UserId != request.UserId).UserId;

        await messageRepository.ClearMessagesByChatAsync(
            request.ChatId, ct);

        await storageService.DeleteChatMessagesFolderAsync(
            chat.Id, ct);
        
        await chatRepository.UpdateLastMessageAsync(
            request.ChatId, 
            null, 
            null, 
            ct);

        await bus.PublishAsync(
            new MessagesByChatClearedEvent(
                recipientId, 
                request.UserId,
                request.ChatId));
    }
}
