using VOID.Domain.Models.Users;

namespace VOID.Application.Abstractions.IServices.IAuthServices;

public interface IJwtProvider
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
}
