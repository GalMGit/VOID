using System.Threading;
using System.Threading.Tasks;
using VOID.Application.Abstractions.IRepositories.IUserRepositories;

namespace VOID.Application.UseCases.Auth.Commands.Logout;

public class LogoutUserCommandHandler(
    IRefreshTokenRepository refreshTokenRepository)
{
    public async Task Handle(
        LogoutUserCommand request, 
        CancellationToken ct)
    {
        var storedToken = await refreshTokenRepository.GetByTokenAsync(
            request.Dto.RefreshToken, ct);

        if (storedToken is not null)
        {
            storedToken.Revoked = true;
            
            await refreshTokenRepository.UpdateAsync(
                storedToken, ct);
        }
    }
}