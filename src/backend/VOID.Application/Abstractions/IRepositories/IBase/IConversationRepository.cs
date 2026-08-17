using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VOID.Shared.Contracts.DTOs.Paginations;

namespace VOID.Application.Abstractions.IRepositories.IBase;

public interface IConversationRepository<T>
{
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<bool> IsMemberAsync(Guid conversationId, Guid userId, CancellationToken ct = default);
    Task<List<T>> GetAllByUserAsync(Guid userId, PaginationRequest pagination, CancellationToken ct = default);
    Task<int> GetTotalCountByUserAsync(Guid userId, CancellationToken ct = default);
    Task UpdateLastMessageAsync(Guid conversationId, string? lastMessage, DateTime? lastMessageDate, CancellationToken ct = default);
}