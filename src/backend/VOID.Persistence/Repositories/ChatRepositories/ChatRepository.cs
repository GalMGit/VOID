using Microsoft.EntityFrameworkCore;
using VOID.Application.Abstractions.IRepositories.IChatRepositories;
using VOID.Domain.Models.Chats;
using VOID.Persistence.Extensions;
using VOID.Persistence.Database.Context;
using VOID.Shared.Contracts.DTOs.Paginations;

namespace VOID.Persistence.Repositories.ChatRepositories;

public class ChatRepository(
    VoidDbContext database) 
    : IChatRepository
{
    public async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken ct = default)
       => await database.Chats
            .AsNoTracking()
            .AnyAsync(x => x.Id == id, ct);
    
    public async Task<bool> ExistsBetweenUsersAsync(
        Guid userId1,
        Guid userId2,
        CancellationToken ct = default)
        => await database.Chats
            .AsNoTracking()
            .AnyAsync(x =>
                x.Interlocutors.Count == 2 &&
                x.Interlocutors.Any(i => i.UserId == userId1) &&
                x.Interlocutors.Any(i => i.UserId == userId2), ct);

    public async Task<Chat> CreateAsync(
        Chat chat,
        CancellationToken ct = default)
    {
        await database.Chats.AddAsync(chat, ct);
        await database.SaveChangesAsync(ct);

        await database.Entry(chat)
            .Collection(x => x.Interlocutors)
            .Query()
            .Include(x => x.User)
            .LoadAsync(ct);

        return chat;
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken ct = default)
        => await database.Chats
            .Where(x => x.Id == id)
            .ExecuteDeleteAsync(ct);
    
    public Task<List<Chat>> GetAllAsync(
        CancellationToken ct = default)
        => throw new NotImplementedException();

    public async Task<List<Chat>> GetAllByUserAsync(
        Guid userId,
        PaginationRequest pagination,
        CancellationToken ct = default)
        => await database.Chats
            .AsNoTracking()
            .Include(x => x.Interlocutors)
                .ThenInclude(x => x.User)
            .Where(x => x.Interlocutors
                .Any(i => i.UserId == userId))
            .OrderByDescending(x => x.LastMessageDate != null)
            .ThenByDescending(x => x.LastMessageDate)
            .ApplyPagination(pagination)
            .ToListAsync(ct);

    public async Task<Chat?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default)
        => await database.Chats
            .AsNoTracking()
            .Include(x => x.Interlocutors)
                .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    
    public async Task<int> GetTotalCountByUserAsync(
        Guid userId,
        CancellationToken ct = default)
        => await database.Chats
            .AsNoTracking()
            .CountAsync(x =>
                x.Interlocutors
                    .Any(i => i.UserId == userId), ct);
    
    public async Task<bool> IsMemberAsync(
        Guid conversationId,
        Guid userId,
        CancellationToken ct = default)
        => await database.Chats
            .AsNoTracking()
            .AnyAsync(x =>
                x.Id == conversationId &&
                x.Interlocutors
                    .Any(i => i.UserId == userId), ct);
    
    public async Task UpdateLastMessageAsync(
        Guid conversationId,
        string? lastMessage,
        DateTime? lastMessageDate,
        CancellationToken ct = default)
        => await database.Chats
            .Where(x => x.Id == conversationId)
            .ExecuteUpdateAsync(x => x
                .SetProperty(c => c.LastMessage, lastMessage)
                .SetProperty(c => c.LastMessageDate,
                    lastMessage != null ? lastMessageDate : null), ct);
    
    public async Task<List<Guid>> GetRelatedUsersIdsAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        var query =
            from interlocutor in database.ChatInterlocutors
            where database.ChatInterlocutors.Any(x =>
                    x.ChatId == interlocutor.ChatId &&
                    x.UserId == userId)
                  && interlocutor.UserId != userId
            select interlocutor.UserId;

        return await query
            .Distinct()
            .ToListAsync(ct);
    }

    public async Task<HashSet<Guid>> GetUsersWithChatsAsync(
        Guid currentUserId,
        List<Guid> targetUserIds,
        CancellationToken ct = default)
    {
        var users = await database.Chats
            .AsNoTracking()
            .Where(chat =>
                chat.Interlocutors.Count == 2 &&
                chat.Interlocutors.Any(i => i.UserId == currentUserId) &&
                chat.Interlocutors
                    .Any(i => targetUserIds
                        .Contains(i.UserId)))
            .SelectMany(chat => chat.Interlocutors)
            .Where(i =>
                i.UserId != currentUserId && targetUserIds
                    .Contains(i.UserId))
            .Select(i => i.UserId)
            .Distinct()
            .ToListAsync(ct);

        return users.ToHashSet();
    }

    public async Task<Guid> GetRecipientIdAsync(
        Guid userId, 
        Guid chatId, 
        CancellationToken ct = default)
        => await database.ChatInterlocutors
            .AsNoTracking()
            .Where(x => x.ChatId == chatId && x.UserId != userId)
            .Select(x => x.UserId)
            .FirstOrDefaultAsync(ct);

    public async Task<bool> HasMediaAsync(
        Guid chatId, 
        CancellationToken ct = default)
        => await database.Messages
            .AnyAsync(x => 
                x.ChatId == chatId
                && x.MediaUrl != null, ct);

    public async Task<Chat?> GetBetweenUsersAsync(
        Guid currentUserId,
        Guid userId,
        CancellationToken ct = default)
        => await database.Chats
            .Where(c => c.Interlocutors.Any(x => 
                            x.UserId == currentUserId)
                        && c.Interlocutors.Any(x => 
                            x.UserId == userId))
            .FirstOrDefaultAsync(ct);

    public Task SoftDeleteAsync(
        Guid id,
        CancellationToken ct = default)
        => throw new NotImplementedException();
    
    public Task<Chat> UpdateAsync(
        Chat entity,
        CancellationToken ct = default)
        => throw new NotImplementedException();
}