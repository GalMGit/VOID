using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VOID.APP.Models.Chat;
using VOID.APP.Models.User;
using VOID.Shared.Contracts.DTOs.Paginations;

namespace VOID.APP.Services.Interfaces.IChat;

public interface IChatService
{
    Task<PaginatedResult<ChatModel>?> GetChatsForUserAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<List<SearchUserResponse>> GetSearchUsers(string username, CancellationToken ct = default);
    Task HardDeleteChatAsync(Guid chatId, CancellationToken ct = default);
    Task CreateChatAsync(string username, CancellationToken ct = default);
    Task ClearChatAsync(Guid chatId, CancellationToken ct = default);
    Task<FullChatModel?> GetChatByIdAsync(Guid chatId, CancellationToken ct = default);
    Task<ChatModel?> GetPrivateChatWithUserAsync(Guid userId, CancellationToken ct = default);
}
