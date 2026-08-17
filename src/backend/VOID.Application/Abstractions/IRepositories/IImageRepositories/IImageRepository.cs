using System;
using System.Threading;
using System.Threading.Tasks;

namespace VOID.Application.Abstractions.IRepositories.IImageRepositories;

public interface IImageRepository
{
    Task UpdateAvatarAsync(string? url, Guid userId, CancellationToken ct = default);
    Task UpdateGroupImageAsync(string? path, Guid groupId, CancellationToken ct = default);
    Task<string?> GetAvatarUrlByUserAsync(Guid userId, CancellationToken ct = default);
}
