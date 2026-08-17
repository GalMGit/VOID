using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using VOID.Application.Abstractions;
using VOID.Application.Abstractions.IRepositories.IChatRepositories;
using VOID.Application.Exceptions;
using VOID.Shared.Contracts.DTOs.Chats;

namespace VOID.Application.UseCases.Chats.Queries.GetWithUser;

public sealed class GetPrivateChatQueryHandler(
    IMapper mapper, 
    IChatRepository chatRepository)
{
    public async Task<ChatDto?> Handle(
        GetPrivateChatQuery request,
        CancellationToken ct = default)
    {
        var chat = await chatRepository.GetBetweenUsersAsync(
            request.CurrentUserId,
            request.UserId, ct) 
                   ?? throw new NotFoundException("Чат не найден");

        return mapper.Map<ChatDto>(chat, 
            opt => opt.Items["CurrentUserId"] 
                = request.CurrentUserId);
    }
}