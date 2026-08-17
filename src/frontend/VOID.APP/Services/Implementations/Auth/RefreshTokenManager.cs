using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using VOID.APP.Services.Interfaces.IAuth;
using VOID.Shared.Contracts.DTOs.Auth.Login;
using VOID.Shared.Contracts.DTOs.Auth.Token;

namespace VOID.APP.Services.Implementations.Auth;

public class RefreshTokenManager
{
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private Task<bool>? _ongoingRefreshTask;

    public async Task<bool> TryRefreshTokenAsync(
        HttpClient refreshHttpClient, 
        ITokenService tokenService)
    {
        if (_ongoingRefreshTask != null && !_ongoingRefreshTask.IsCompleted)
            return await _ongoingRefreshTask;

        await _refreshLock.WaitAsync();
        try
        {
            if (_ongoingRefreshTask != null && !_ongoingRefreshTask.IsCompleted)
                return await _ongoingRefreshTask;

            _ongoingRefreshTask = RefreshTokenAsync(
                refreshHttpClient, 
                tokenService);
            
            return await _ongoingRefreshTask;
        }
        finally
        {
            _ongoingRefreshTask = null;
            _refreshLock.Release();
        }
    }

    private async Task<bool> RefreshTokenAsync(
        HttpClient refreshHttpClient, 
        ITokenService tokenService)
    {
        if (string.IsNullOrWhiteSpace(tokenService.RefreshToken))
            return false;

        try
        {
            var response = await refreshHttpClient.PostAsJsonAsync(
                "auth/refresh",
                new RefreshTokenDto { RefreshToken = tokenService.RefreshToken });

            if (!response.IsSuccessStatusCode)
                return false;

            var result = await response.Content.ReadFromJsonAsync<LoginDto>();
            if (result is null)
                return false;

            await tokenService.SaveTokensAsync(
                result.Token, 
                result.RefreshToken);
            
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}