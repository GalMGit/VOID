using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using VOID.Application.Abstractions.IServices.IAuthServices;
using VOID.Domain.Models.Users;

namespace VOID.Infrastructure.Auth;

public class JwtProvider(
    IOptions<JwtOptions> options
    ) : IJwtProvider
{
    private readonly JwtOptions _options = options.Value;

    public string GenerateRefreshToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    
    public string GenerateToken(User user)
    {
        Claim[] claims = [
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.AppRole.ToString())
        ];

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            signingCredentials: signingCredentials,
            expires: DateTime.UtcNow.AddMinutes(_options.Expires),
            claims: claims);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}
