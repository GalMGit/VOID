using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using VOID.Application.Abstractions.IServices.IMediaServices;

namespace VOID.Infrastructure.Storage;

public sealed class PrivateStorage : S3ObjectStorage, IPrivateStorage
{
    private readonly PrivateStorageOptions _options;

    public PrivateStorage(
        IAmazonS3 client,
        IOptions<PrivateStorageOptions> options)
        : base(client, options.Value.Bucket)
    {
        _options = options.Value;
    }


    public string GetPresignedUrl(string key, TimeSpan lifetime)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.Bucket,
            Key = key,
            Expires = DateTime.UtcNow.Add(lifetime)
        };

        return Client.GetPreSignedURL(request);
    }
}