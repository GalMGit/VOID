using System;
using Microsoft.EntityFrameworkCore;
using VOID.Application.Abstractions.IRepositories.IUserRepositories;
using VOID.Domain.Models.Users;
using VOID.Persistence.Database.Context;

namespace VOID.Persistence.Repositories.UserRepositories;

public class RefreshTokenRepository(
    VoidDbContext database
    ) : IRefreshTokenRepository
{
    public async Task<RefreshToken> CreateAsync(
        RefreshToken entity, 
        CancellationToken ct = default)
    {
        await database.AddAsync(entity, ct);
        await database.SaveChangesAsync(ct);
        return entity;
    }

    public Task DeleteAsync(
        Guid id, 
        CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<RefreshToken>> GetAllAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<RefreshToken?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<RefreshToken?> GetByTokenAsync(
        string refreshToken, 
        CancellationToken ct = default)
    {
        return await database.RefreshTokens
            .Where(x => x.Token == refreshToken)
            .Include(s => s.User)
            .FirstOrDefaultAsync(ct);
    }

    public Task SoftDeleteAsync(
        Guid id, 
        CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<RefreshToken> UpdateAsync(
        RefreshToken entity, 
        CancellationToken ct = default)
    {
        database.Update(entity);
        await database.SaveChangesAsync(ct);
        return entity;
    }
}
