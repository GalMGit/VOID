using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using VOID.APP.Models.User;

namespace VOID.APP.Services.Interfaces.IAuth;

public interface ITokenService
{
    string? AccessToken { get; set; }
    string? RefreshToken { get; set; }

    void ClearToken();

    Task ClearStoredTokenAsync();

    bool IsTokenValid();

    Task SaveTokensAsync(
        string? accessToken,
        string? refreshToken);

    Task<bool> HasValidTokensAsync();

    Task LoadTokensAsync();

    JwtSecurityToken? DecodeToken();

    string? GetClaim(string claimType);

    AuthUser GetUserInfoFromToken();
}