using Microsoft.EntityFrameworkCore;
using VOID.Application.Abstractions.IRepositories.IMessageRepositories;
using VOID.Domain.Enums.Types.Chat;
using VOID.Domain.Models.Messages;
using VOID.Persistence.Extensions;
using VOID.Persistence.Database.Context;
using VOID.Shared.Contracts.DTOs.Paginations;

namespace VOID.Persistence.Repositories.MessageRepositories;

public class MessageRepository(
    VoidDbContext database) 
    : IMessageRepository
{
    public async Task<Message> CreateAsync(
        Message message, 
        CancellationToken ct = default)
    {
        await database.Messages.AddAsync(message, ct);
        await database.SaveChangesAsync(ct);

        await database.Entry(message)
            .Reference(x => x.Sender)
            .LoadAsync(ct);

        return message;
    }

    public async Task<Message?> GetLastMessageAsync(
        Guid chatId, 
        CancellationToken ct = default)
        => await database.Messages
            .AsNoTracking()
            .Where(x => x.ChatId == chatId)
            .Include(u => u.Sender)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);
    
    public async Task ReadMessagesAsync(
        Guid chatId, 
        Guid userId, 
        ChatType chatType, 
        CancellationToken ct = default)
    {
        if (chatType == ChatType.Private)
        {
            await database.Messages
                .Where(x => 
                    x.ChatId == chatId 
                    && x.SenderId != userId 
                    && !x.IsRead)
                .ExecuteUpdateAsync(x => x
                    .SetProperty(s => 
                        s.IsRead, true)
                    .SetProperty(s => 
                        s.ReadAt, DateTime.UtcNow), ct);
        }
        else if (chatType == ChatType.Group)
        {
            await database.Messages
                .Where(x => 
                    x.GroupChatId == chatId 
                    && x.SenderId != userId 
                    && !x.IsRead)
                .ExecuteUpdateAsync(x => x
                    .SetProperty(s => 
                        s.IsRead, true)
                    .SetProperty(s => 
                        s.ReadAt, DateTime.UtcNow), ct);
        }
    }

    public async Task DeleteAsync(
        Guid id, 
        CancellationToken ct = default)
        => await database.Messages
            .Where(x => x.Id == id)
            .ExecuteDeleteAsync(ct);
    
    public Task<List<Message>> GetAllAsync(CancellationToken ct = default)
        => throw new NotImplementedException();
    
    public async Task<Message?> GetByIdAsync(
        Guid id, 
        CancellationToken ct = default)
        => await database.Messages
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Dictionary<Guid, int>> GetUnreadCountsAsync(
        Guid userId,
        List<Guid> chatIds,
        CancellationToken ct = default)
        => await database.Messages
            .Where(x =>
                x.ChatId.HasValue && 
                chatIds.Contains(x.ChatId.Value) &&
                x.SenderId != userId &&
                !x.IsRead)
            .GroupBy(x => x.ChatId!.Value)
            .Select(x => new
            {
                ChatId = x.Key,
                Count = x.Count()
            })
            .ToDictionaryAsync(
                x => x.ChatId,
                x => x.Count,
                ct);
    

    public async Task<List<Message>> GetMessagesByParentAsync(
        Guid parentId,
        ChatType chatType,
        PaginationRequest pagination,
        CancellationToken ct = default)
        => chatType switch
        {
            ChatType.Private => await database.Messages.AsNoTracking()
                .Include(x => x.Sender)
                .Where(x => x.ChatId == parentId)
                .OrderByDescending(x => x.CreatedAt)
                .ApplyPagination(pagination)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync(ct),
            
            ChatType.Group => await database.Messages.AsNoTracking()
                .Include(x => x.Sender)
                .Where(x => x.GroupChatId == parentId)
                .OrderByDescending(x => x.CreatedAt)
                .ApplyPagination(pagination)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync(ct),
            _ => []
        };

    public async Task<int> GetTotalCountByChatAsync(
        Guid chatId,
        CancellationToken ct = default)
        => await database.Messages
            .AsNoTracking()
            .Where(x => x.ChatId == chatId)
            .CountAsync(ct);
    
    public async Task<int> GetTotalCountByGroupAsync(
        Guid groupId, 
        CancellationToken ct = default)
        => await database.Messages
            .AsNoTracking()
            .Where(x => x.GroupChatId == groupId)
            .CountAsync(ct);
    
    public async Task ClearMessagesByChatAsync(
        Guid chatId,
        CancellationToken ct = default)
        => await database.Messages
            .Where(x => x.ChatId == chatId)
            .ExecuteDeleteAsync(ct);

    public Task SoftDeleteAsync(
        Guid id,
        CancellationToken ct = default)
        => throw new NotImplementedException();
    
    public async Task<Message> UpdateAsync(
        Message message,
        CancellationToken ct = default)
    {
        database.Update(message);
        await database.SaveChangesAsync(ct);
        await database.Entry(message)
            .Reference(x => x.Sender)
            .LoadAsync(ct);
        return message;
    }
}
