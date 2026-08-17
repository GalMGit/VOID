using Microsoft.EntityFrameworkCore;
using VOID.Application.Abstractions.IRepositories.IImageRepositories;
using VOID.Persistence.Database.Context;

namespace VOID.Persistence.Repositories.ImageRepositories;

public class ImageRepository(
    VoidDbContext database) 
    : IImageRepository
{
    public async Task UpdateAvatarAsync(
        string? url, 
        Guid userId, 
        CancellationToken ct = default)
        => await database.Users
            .Where(x => x.Id == userId)
            .ExecuteUpdateAsync(x => x
                .SetProperty(p => p.AvatarUrl, url), ct);

    public async Task UpdateGroupImageAsync(
        string? path,
        Guid groupId,
        CancellationToken ct = default)
        => await database.Groups
            .Where(x => x.Id == groupId)
            .ExecuteUpdateAsync(x => x
                .SetProperty(p => p.ImageUrl, path), ct);

    public async Task<string?> GetAvatarUrlByUserAsync(
        Guid userId,
        CancellationToken ct = default)
        => await database.Users
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => x.AvatarUrl)
            .FirstOrDefaultAsync(ct);
}
