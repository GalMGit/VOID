using Microsoft.Extensions.Options;
using VOID.Application.Abstractions.IServices.IMediaServices;

namespace VOID.Infrastructure.Storage;

public sealed class MediaUrlService(
    IOptions<ApiOptions> options, 
    IPublicStorage publicStorage,
    IPrivateStorage privateStorage) 
    : IMediaUrlService
{
    private readonly ApiOptions _options = options.Value;
    
    public string? GetAvatarUrl(string? relativePath)
        => string.IsNullOrWhiteSpace(relativePath) 
            ? null 
            : publicStorage.GetPublicUrl(relativePath);
    
    public string GetMessageMediaUrl(Guid messageId)
        => $"{_options.BaseUrl}{_options.ApiPrefix}/messages/{messageId}/media";

    public string GetMessageThumbnailUrl(Guid messageId)
        => $"{_options.BaseUrl}{_options.ApiPrefix}/messages/{messageId}/thumbnail";
}