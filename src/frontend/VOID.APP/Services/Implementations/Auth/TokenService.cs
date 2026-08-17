using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VOID.APP.Models.User;
using VOID.APP.Services.Interfaces.IAuth;

namespace VOID.APP.Services.Implementations.Auth;

public class TokenService : ITokenService
{
    private readonly ITokenStorageService _storageService;

    public string? AccessToken { get; set; }

    public string? RefreshToken { get; set; }

    public TokenService(ITokenStorageService storageService)
        => _storageService = storageService;
    

    public JwtSecurityToken? DecodeToken()
    {
        if (string.IsNullOrWhiteSpace(AccessToken))
            return null;

        try
        {
            return new JwtSecurityTokenHandler().ReadJwtToken(AccessToken);
        }
        catch
        {
            return null;
        }
    }

    public string? GetClaim(string claimType)
    {
        var jwt = DecodeToken();

        return jwt?.Claims.FirstOrDefault(x => 
                x.Type == claimType)?.Value;
    }

    public AuthUser GetUserInfoFromToken()
    {
        var user = new AuthUser();

        var jwt = DecodeToken();

        if (jwt is null)
            return user;

        user.Username = jwt.Claims.FirstOrDefault(x => 
                x.Type == ClaimTypes.Name)?.Value;

        if (Guid.TryParse(jwt.Claims.FirstOrDefault(x => 
                x.Type == ClaimTypes.NameIdentifier)?.Value, out var id))
            user.Id = id;
        

        user.Email = jwt.Claims.FirstOrDefault(x => 
                x.Type == ClaimTypes.Email)?.Value;

        user.AppRole = jwt.Claims.FirstOrDefault(x => 
                x.Type == ClaimTypes.Role)?.Value;

        user.IsAuthenticated = jwt.ValidTo > DateTime.UtcNow;

        return user;
    }

    public bool IsTokenValid()
    {
        var jwt = DecodeToken();

        return jwt is not null &&
               jwt.ValidTo > DateTime.UtcNow;
    }

    public async Task SaveTokensAsync(
        string? accessToken,
        string? refreshToken)
    {
        await _storageService.SaveTokensAsync(
            accessToken,
            refreshToken);

        AccessToken = accessToken;
        RefreshToken = refreshToken;
    }

    public async Task LoadTokensAsync()
    {
        var data = await _storageService.LoadTokensAsync();

        if (data is null)
            return;

        AccessToken = data.Token;
        RefreshToken = data.RefreshToken;
    }

    public void ClearToken()
    {
        AccessToken = null;
        RefreshToken = null;
    }

    public async Task ClearStoredTokenAsync()
    {
        await _storageService.ClearTokenAsync();
        ClearToken();
    }

    public Task<bool> HasValidTokensAsync()
    {
        return Task.FromResult(
            !string.IsNullOrWhiteSpace(AccessToken) &&
            !string.IsNullOrWhiteSpace(RefreshToken));
    }
}