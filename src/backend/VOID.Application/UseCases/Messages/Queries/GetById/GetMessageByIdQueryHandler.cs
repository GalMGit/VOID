using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using VOID.Application.Abstractions.IRepositories.IMessageRepositories;
using VOID.Shared.Contracts.DTOs.Messages;

namespace VOID.Application.UseCases.Messages.Queries.GetById;

public sealed class GetMessageByIdQueryHandler(
    IMessageRepository messageRepository,
    IMapper mapper)
{
    public async Task<MessageDto?> Handle(
        GetMessageByIdQuery request,
        CancellationToken ct)
    {
        var message = await messageRepository.GetByIdAsync(
                request.MessageId,
                ct);

        return mapper.Map<MessageDto>(message,
            opt => opt.Items["CurrentUserId"] = request.UserId);
    }
}