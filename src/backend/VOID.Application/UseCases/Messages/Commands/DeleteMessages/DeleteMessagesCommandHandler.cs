using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VOID.Application.Abstractions.IRepositories.IMessageRepositories;
using VOID.Application.Exceptions;

namespace VOID.Application.UseCases.Messages.Commands.DeleteMessages;

public sealed class DeleteMessagesCommandHandler(
    IMessageRepository messageRepository)
{
    public async Task Handle(
        DeleteMessagesCommand request,
        CancellationToken ct)
    {
        var messageIds = request.Dto.MessageIds
            .Distinct()
            .ToArray();
        
        if (messageIds.Length == 0)
            return;
        
        var messages = await messageRepository.GetByIds(
            messageIds,
            request.UserId, ct);
        
        if (messages.Count != messageIds.Length)
            throw new ForbiddenException();
        
        await messageRepository.DeleteRangeAsync(
            messageIds,
            ct);
    }
}