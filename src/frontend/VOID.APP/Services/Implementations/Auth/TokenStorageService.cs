using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using VOID.APP.Models.Tokens;
using VOID.APP.Services.Interfaces.IAuth;

namespace VOID.APP.Services.Implementations.Auth;

public class TokenStorageService : ITokenStorageService
{
    private readonly string _filePath;

    public TokenStorageService()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NothingAPP");

        Directory.CreateDirectory(appDataPath);

        _filePath = Path.Combine(
            appDataPath, 
            "auth.json");
    }

    public async Task SaveTokensAsync(
        string? accessToken,
        string? refreshToken)
    {
        var authData = new AuthData
        {
            Token = accessToken,
            RefreshToken = refreshToken
        };

        var json = JsonSerializer.Serialize(authData);

        await System.IO.File.WriteAllTextAsync(
            _filePath, 
            json);
    }

    public async Task<AuthData?> LoadTokensAsync()
    {
        if (!System.IO.File.Exists(_filePath))
            return null;

        try
        {
            var json = await System.IO.File.ReadAllTextAsync(_filePath);

            return JsonSerializer.Deserialize<AuthData>(json);
        }
        catch
        {
            return null;
        }
    }

    public Task ClearTokenAsync()
    {
        if (System.IO.File.Exists(_filePath))
            System.IO.File.Delete(_filePath);

        return Task.CompletedTask;
    }
}
