using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using VOID.Application.Abstractions.IRepositories.IChatRepositories;
using VOID.Application.Abstractions.IRepositories.IMessageRepositories;
using VOID.Application.Exceptions;
using VOID.Shared.Contracts.DTOs.Chats;

namespace VOID.Application.UseCases.Chats.Queries.GetById;

public sealed class GetChatByIdQueryHandler(
    IChatRepository chatRepository,
    IMessageRepository messageRepository,
    IMapper mapper)
{
    public async Task<FullChatDto> Handle(
        GetChatByIdQuery request, 
        CancellationToken ct)
    {
        var chat = await chatRepository.GetByIdAsync(
                       request.ChatId, ct)
                   ?? throw new NotFoundException("Чат не найден");

        if (chat.Interlocutors
            .All(x => x.UserId != request.UserId))
            throw new ForbiddenException();

        var messageCount = await messageRepository.GetTotalCountByChatAsync(
            chat.Id, ct);

        var mappedChat = mapper.Map<FullChatDto>(chat, opts =>
            opts.Items["CurrentUserId"] = request.UserId);

        mappedChat.MessageCount = messageCount;

        return mappedChat;
    }
}