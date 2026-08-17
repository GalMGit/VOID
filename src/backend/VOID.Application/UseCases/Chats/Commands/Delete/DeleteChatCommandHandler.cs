using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VOID.Application.Abstractions.IRepositories.IChatRepositories;
using VOID.Application.Abstractions.IServices.IMediaServices;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Chats.Events.Deleted;
using Wolverine;

namespace VOID.Application.UseCases.Chats.Commands.Delete;

public sealed class DeleteChatCommandHandler(
    IChatRepository chatRepository,
    IFileStorageService storageService,
    IMessageBus bus)
{
    public async Task Handle(
        DeleteChatCommand request, 
        CancellationToken ct)
    {
        var chat = await chatRepository.GetByIdAsync(
                       request.ChatId, ct)
                   ?? throw new NotFoundException("Чат не найден");

        var isMember = chat.Interlocutors
            .Any(x => x.UserId == request.UserId);

        if (!isMember)
            throw new ForbiddenException();

        var recipientId = await chatRepository.GetRecipientIdAsync(
                request.UserId, 
                request.ChatId, ct);
        
        if(await chatRepository.HasMediaAsync(chat.Id, ct))
            await storageService.DeleteChatMessagesFolderAsync(
                chat.Id, ct);

        await chatRepository.DeleteAsync(
            request.ChatId, ct);
        
        await bus.PublishAsync(
            new ChatDeletedEvent(
                recipientId,
                request.ChatId, 
                request.UserId));
    }
}