using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VOID.Application.Abstractions.IRepositories.IBase;
using VOID.Domain.Enums.Types.Chat;
using VOID.Domain.Models.Messages;
using VOID.Shared.Contracts.DTOs.Paginations;

namespace VOID.Application.Abstractions.IRepositories.IMessageRepositories;

public interface IMessageRepository : IRepository<Message>
{
    Task<int> GetTotalCountByChatAsync(Guid chatId, CancellationToken ct = default);
    Task ClearMessagesByChatAsync(Guid chatId, CancellationToken ct = default);
    Task ReadMessagesAsync(Guid chatId, Guid userId, ChatType chatType, CancellationToken ct = default);
    Task<Message?> GetLastMessageAsync(Guid chatId, CancellationToken ct = default);
    Task<int> GetTotalCountByGroupAsync(Guid groupId, CancellationToken ct = default);
    Task<Dictionary<Guid, int>> GetUnreadCountsAsync(Guid userId, List<Guid> chatIds, CancellationToken ct = default);
    Task<List<Message>> GetMessagesByParentAsync(Guid parentId,ChatType chatType, PaginationRequest pagination, CancellationToken ct = default);
}
