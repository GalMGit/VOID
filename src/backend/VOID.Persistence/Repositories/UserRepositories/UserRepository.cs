using System;
using Microsoft.EntityFrameworkCore;
using VOID.Application.Abstractions.IRepositories.IUserRepositories;
using VOID.Domain.Models.Users;
using VOID.Persistence.Database.Context;

namespace VOID.Persistence.Repositories.UserRepositories;

public class UserRepository(
    VoidDbContext database) 
    : IUserRepository
{
    public async Task<User> CreateAsync(
        User user,
        CancellationToken ct = default)
    {
        await database.Users.AddAsync(user, ct);
        await database.SaveChangesAsync(ct);
        return user;
    }

    public async Task OnlineStatusChangeAsync(
        Guid userId, 
        bool isOnline, 
        CancellationToken ct = default)
    {
        await database.Users
            .Where(x => x.Id == userId)
            .ExecuteUpdateAsync(x => x
                .SetProperty(s => s.IsOnline, isOnline), ct);
    }

    public async Task ChangePasswordAsync(
        Guid userId,
        string newPassword,
        CancellationToken ct = default)
        => await database.Users
            .Where(x => x.Id == userId && !x.IsDeleted)
            .ExecuteUpdateAsync(x => 
                x.SetProperty(p => p.PasswordHash, newPassword), ct);

    public async Task<List<User>> SearchUsersForGroupAsync(
        string searchTerm,
        Guid currentUserId,
        Guid groupId,
        CancellationToken ct = default)
    {
        return await database.Users
            .AsNoTracking()
            .Where(user =>
                user.Id != currentUserId
                &&
                user.Username.ToLower()
                    .Equals(searchTerm.ToLower())
                &&
                database.Chats.Any(chat =>
                    chat.Interlocutors.Count == 2 &&
                    chat.Interlocutors
                        .Any(i => i.UserId == currentUserId) &&
                    chat.Interlocutors
                        .Any(i => i.UserId == user.Id)
                    )
                && !database.GroupMembers
                    .Any(member =>
                        member.GroupId == groupId &&
                        member.MemberId == user.Id)
            )
            .Take(20)
            .ToListAsync(ct);
    }

    public async Task<bool> UsernameExistsAsync(
        string username, 
        CancellationToken ct = default)
    {
        return await database.Users
            .AsNoTracking()
            .AnyAsync(u =>
                u.Username.ToLower()
                    .Equals(username.ToLower()), ct);
    }

    public async Task<bool> EmailExistsAsync(
        string email, 
        CancellationToken ct = default)
    {
        return await database.Users
            .AsNoTracking()
            .AnyAsync(u =>
                u.Email.ToLower()
                    .Equals(email.ToLower()), ct);
    }

    public Task DeleteAsync(
        Guid id, 
        CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<User>> GetAllAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<User?> GetByIdAsync(
        Guid id, 
        CancellationToken ct = default)
    {
        return await database.Users
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<User?> GetByEmailAsync(
        string email, 
        CancellationToken ct = default)
    {
        return await database.Users
            .AsNoTracking()
            .Where(x => 
                x.Email.ToLower()
                    .Equals(email.ToLower()))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<User?> GetByUsernameAsync(
        string username, 
        CancellationToken ct = default)
    {
        return await database.Users
            .AsNoTracking()
            .Where(x => 
                x.Username.ToLower()
                    .Equals(username.ToLower()))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<User>> SearchAsync(
        string searchTerm,
        Guid userId, 
        CancellationToken ct = default)
    {
        return await database.Users
            .Where(x => 
                x.Username.ToLower()
                    .Equals(searchTerm.ToLower())
                && x.Id != userId)
            .ToListAsync(ct);
    }

    public async Task ChangeUserLastSeenAsync(
        Guid userId, 
        CancellationToken ct = default)
    {
        await database.Users
            .Where(x => x.Id == userId)
            .ExecuteUpdateAsync(x => x
                .SetProperty(s => s.LastSeen, DateTime.UtcNow), ct);
    }

    public Task SoftDeleteAsync(
        Guid id, 
        CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<User> UpdateAsync(
        User user, 
        CancellationToken ct = default)
    {
        database.Update(user);
        await database.SaveChangesAsync(ct);
        return user;
    }
}
