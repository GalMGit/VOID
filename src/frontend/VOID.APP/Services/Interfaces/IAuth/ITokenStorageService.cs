using System;
using System.Threading.Tasks;
using VOID.APP.Models.Tokens;

namespace VOID.APP.Services.Interfaces.IAuth;

public interface ITokenStorageService
{
    Task SaveTokensAsync(
    string? accessToken,
    string? refreshToken);
    Task<AuthData?> LoadTokensAsync();
    Task ClearTokenAsync();
}