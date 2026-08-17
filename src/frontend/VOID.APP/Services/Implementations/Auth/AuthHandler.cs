using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using VOID.APP.Services.Interfaces.IAuth;

namespace VOID.APP.Services.Implementations.Auth;

public class AuthHandler : DelegatingHandler
{
    private readonly ITokenService _tokenService;
    private readonly IAuthErrorHandler _errorHandler;
    private readonly RefreshTokenManager _refreshManager;
    private readonly HttpClient _refreshHttpClient;

    public AuthHandler(
        ITokenService tokenService,
        IAuthErrorHandler errorHandler,
        RefreshTokenManager refreshManager)
    {
        _tokenService = tokenService;
        _errorHandler = errorHandler;
        _refreshManager = refreshManager;

        _refreshHttpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5018/api/"),
            Timeout = TimeSpan.FromMinutes(1)
        };
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"Отправка запроса: {request.Method} {request.RequestUri}");

        var isApiRequest = request.RequestUri?.Host == "localhost";

        if (isApiRequest &&
            !string.IsNullOrWhiteSpace(_tokenService.AccessToken))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    _tokenService.AccessToken);
        }

        var response = await base.SendAsync(
            request,
            cancellationToken);

        if (isApiRequest &&
            response.StatusCode == HttpStatusCode.Unauthorized)
        {
            Console.WriteLine("Получен 401, пробуем обновить токен");

            var refreshSuccess = await _refreshManager.TryRefreshTokenAsync(
                _refreshHttpClient,
                _tokenService);

            Console.WriteLine($"Результат refresh: {refreshSuccess}");

            if (refreshSuccess)
            {
                Console.WriteLine("Токен обновлен, создаем клон запроса и повторяем");

                var clonedRequest = await CloneHttpRequestMessageAsync(request);

                clonedRequest.Headers.Authorization = new AuthenticationHeaderValue(
                        "Bearer",
                        _tokenService.AccessToken);

                response.Dispose();
                response = await base.SendAsync(
                    clonedRequest, cancellationToken);
                Console.WriteLine($"Повторный запрос завершен с кодом: {response.StatusCode}");
            }
            else
            {
                Console.WriteLine("Refresh не удался, очищаем токены и редиректим на логин");
                await _tokenService.ClearStoredTokenAsync();
                _errorHandler.HandleUnauthorized();
            }
        }

        return response;
    }

    private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(
        HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(
            request.Method,
            request.RequestUri);

        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(
                header.Key,
                header.Value);


        clone.Version = request.Version;
        clone.VersionPolicy = request.VersionPolicy;

        if (request.Content != null)
        {
            var contentStream = new MemoryStream();
            await request.Content.CopyToAsync(contentStream);
            contentStream.Position = 0;

            clone.Content = new StreamContent(contentStream);

            foreach (var header in request.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _refreshHttpClient?.Dispose();

        base.Dispose(disposing);
    }
}