using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using VOID.Application.Abstractions.IRepositories.IChatRepositories;
using VOID.Application.Abstractions.IRepositories.IMessageRepositories;
using VOID.Application.Abstractions.IServices.ISecurityServices;
using VOID.Shared.Contracts.DTOs.Chats;
using VOID.Shared.Contracts.DTOs.Paginations;

namespace VOID.Application.UseCases.Chats.Queries.GetChatsByUser;

public sealed class GetChatsByUserQueryHandler(
    IChatRepository chatRepository,
    IMessageRepository messageRepository,
    IEncryptionService encryptionService,
    IMapper mapper)
{
    public async Task<PaginatedResult<ChatDto>> Handle(
        GetChatsByUserQuery request, 
        CancellationToken ct)
    {
        var totalCount = await chatRepository.GetTotalCountByUserAsync(
            request.UserId, ct);

        var chats = await chatRepository.GetAllByUserAsync(
                request.UserId, 
                request.Pagination,
                ct);

        var chatIds = chats
            .Select(x => x.Id)
            .ToList();

        var unreadCounts = await messageRepository.GetUnreadCountsAsync(
            request.UserId,
            chatIds,
            ct);

        var chatsResponse = mapper.Map<List<ChatDto>>(chats, opts =>
            opts.Items["CurrentUserId"] = request.UserId);
        
        foreach (var chat in chatsResponse)
        {
            if (!string.IsNullOrWhiteSpace(chat.LastMessage))
                chat.LastMessage = encryptionService.Decrypt(chat.LastMessage);

            chat.UnreadCount = unreadCounts.GetValueOrDefault(chat.Id);
        }

        return new PaginatedResult<ChatDto>(
            chatsResponse,
            totalCount,
            request.Pagination.PageNumber,
            request.Pagination.PageSize
        );
    }
}