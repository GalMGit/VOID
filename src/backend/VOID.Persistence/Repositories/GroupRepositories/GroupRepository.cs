using Microsoft.EntityFrameworkCore;
using VOID.Application.Abstractions.IRepositories.IGroupRepositories;
using VOID.Domain.Enums.Roles.Group;
using VOID.Domain.Models.Groups;
using VOID.Persistence.Extensions;
using VOID.Persistence.Database.Context;
using VOID.Shared.Contracts.DTOs.Paginations;

namespace VOID.Persistence.Repositories.GroupRepositories;

public class GroupRepository(
    VoidDbContext database) 
    : IGroupRepository
{
    public async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken ct = default)
    {
        return await database.Groups
            .AsNoTracking()
            .AnyAsync(x => x.Id == id, ct);
    }

    public async Task<bool> GroupNameExistsAsync(
        string groupName,
        CancellationToken ct = default)
    {
        return await database.Groups
            .AsNoTracking()
            .AnyAsync(x =>
                x.ChatName.ToLower() == groupName.ToLower(), ct);
    }

    public async Task<GroupChat> CreateAsync(
        GroupChat group,
        CancellationToken ct = default)
    {
        await database.Groups.AddAsync(group, ct);
        await database.SaveChangesAsync(ct);

        return group;
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken ct = default)
    {
        await database.Groups
            .Where(x => x.Id == id)
            .ExecuteDeleteAsync(ct);
    }

    public Task<List<GroupChat>> GetAllAsync(
        CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<GroupChat?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default)
    {
        return await database.Groups
            .AsNoTracking()
            .Include(x => x.GroupMembers)
                .ThenInclude(x => x.Member)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<List<GroupChat>> GetAllByUserAsync(
        Guid userId,
        PaginationRequest pagination,
        CancellationToken ct = default)
    {
        return await database.Groups
            .AsNoTracking()
            .Where(x => x.GroupMembers
                .Any(m => m.MemberId == userId))
            .OrderByDescending(x => x.LastMessageDate != null)
            .ThenByDescending(x => x.LastMessageDate)
            .ApplyPagination(pagination)
            .ToListAsync(ct);
    }

    public async Task<int> GetTotalCountByUserAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        return await database.Groups
            .AsNoTracking()
            .CountAsync(x =>
                x.GroupMembers
                    .Any(m => m.MemberId == userId), ct);
    }

    public async Task<bool> IsMemberAsync(
        Guid conversationId,
        Guid userId,
        CancellationToken ct = default)
    {
        return await database.GroupMembers
            .AsNoTracking()
            .AnyAsync(x =>
                x.GroupId == conversationId &&
                x.MemberId == userId, ct);
    }

    public async Task UpdateLastMessageAsync(
        Guid conversationId,
        string? lastMessage,
        DateTime? lastMessageDate,
        CancellationToken ct = default)
    {
        await database.Groups
            .Where(x => x.Id == conversationId)
            .ExecuteUpdateAsync(x => x
                .SetProperty(g => g.LastMessageDate,
                    lastMessage != null ? lastMessageDate : null), ct);
    }

    public async Task AddMembersRangeAsync(
        List<GroupMember> members,
        CancellationToken ct = default)
    {
        await database.GroupMembers.AddRangeAsync(members, ct);
        await database.SaveChangesAsync(ct);
    }

    public async Task<List<GroupMember>> GetMembersWithDetailsAsync(
        Guid groupId,
        List<Guid> memberIds,
        CancellationToken ct = default)
    {
        return await database.GroupMembers
            .Include(x => x.Member)
            .Where(x =>
                x.GroupId == groupId && memberIds
                    .Contains(x.MemberId))
            .ToListAsync(ct);
    }

    public async Task<HashSet<Guid>> GetExistingMemberIdsAsync(
        Guid groupId,
        List<Guid> userIds,
        CancellationToken ct = default)
    {
        var members = await database.GroupMembers
            .Where(x =>
                x.GroupId == groupId && userIds
                    .Contains(x.MemberId))
            .Select(x => x.MemberId)
            .ToListAsync(ct);

        return [.. members];
    }

    public async Task<bool> IsOwnerAsync(
        Guid groupId,
        Guid userId,
        CancellationToken ct = default)
    {
        return await database.GroupMembers
            .AsNoTracking()
            .AnyAsync(x =>
                x.GroupId == groupId &&
                x.MemberId == userId &&
                x.GroupRole == GroupRole.Owner, ct);
    }

    public async Task DeleteMemberAsync(
        Guid groupId,
        Guid memberId,
        CancellationToken ct = default)
    {
        await database.GroupMembers
            .Where(x =>
                x.GroupId == groupId &&
                x.MemberId == memberId)
            .ExecuteDeleteAsync(ct);
    }

    public async Task<int> GetTotalCountOwnedAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        return await database.Groups
            .AsNoTracking()
            .CountAsync(x => x.OwnerId == userId, ct);
    }

    public Task<GroupChat> UpdateAsync(
        GroupChat entity,
        CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task SoftDeleteAsync(
        Guid id,
        CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}