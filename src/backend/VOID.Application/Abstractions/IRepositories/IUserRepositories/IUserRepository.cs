using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VOID.Application.Abstractions.IRepositories.IBase;
using VOID.Domain.Models.Users;

namespace VOID.Application.Abstractions.IRepositories.IUserRepositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<List<User>> SearchAsync(string searchTerm, Guid userId, CancellationToken ct = default);
    Task ChangeUserLastSeenAsync(Guid userId, CancellationToken ct = default);
    Task OnlineStatusChangeAsync(Guid userId, bool isOnline, CancellationToken ct = default);
    Task ChangePasswordAsync(Guid userId, string newPassword, CancellationToken ct = default);
    Task<List<User>> SearchUsersForGroupAsync(string searchTerm, Guid currentUserId, Guid groupId, CancellationToken ct = default);
}
