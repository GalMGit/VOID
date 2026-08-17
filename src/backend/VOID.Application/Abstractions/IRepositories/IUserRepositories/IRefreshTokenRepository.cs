using System.Threading;
using System.Threading.Tasks;
using VOID.Application.Abstractions.IRepositories.IBase;
using VOID.Domain.Models.Users;

namespace VOID.Application.Abstractions.IRepositories.IUserRepositories;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string refreshToken, CancellationToken ct = default);
}
