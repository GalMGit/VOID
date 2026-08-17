using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VOID.Application.Abstractions.IRepositories.IBase;
using VOID.Domain.Models.Chats;

namespace VOID.Application.Abstractions.IRepositories.IChatRepositories;

public interface IChatRepository :
    IRepository<Chat>,
    IConversationRepository<Chat>
{
    Task<bool> ExistsBetweenUsersAsync(Guid userId1, Guid userId2, CancellationToken ct = default);
    Task<List<Guid>> GetRelatedUsersIdsAsync(Guid userId, CancellationToken ct = default);
    Task<HashSet<Guid>> GetUsersWithChatsAsync(Guid currentUserId, List<Guid> targetUserIds, CancellationToken ct = default);
    Task<Guid> GetRecipientIdAsync(Guid userId, Guid chatId, CancellationToken ct = default);
    Task<bool> HasMediaAsync(Guid chatId, CancellationToken ct = default);
    Task<Chat?> GetBetweenUsersAsync(Guid currentUserId, Guid userId, CancellationToken ct = default);
}