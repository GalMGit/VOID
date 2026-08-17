using Amazon.S3;
using Microsoft.Extensions.Options;
using VOID.Application.Abstractions.IServices.IMediaServices;

namespace VOID.Infrastructure.Storage;

public sealed class PublicStorage : S3ObjectStorage, IPublicStorage
{
    private readonly PublicStorageOptions _options;

    public PublicStorage(
        IAmazonS3 client,
        IOptions<PublicStorageOptions> options)
        : base(client, options.Value.Bucket)
    {
        _options = options.Value;
    }
    
    public string GetPublicUrl(string key)
        => $"{_options.BaseUrl}/{key}";
}