using Amazon.S3;

namespace VOID.Infrastructure.Storage;

public sealed class S3ClientFactory
{
    public IAmazonS3 Create(
        string serviceUrl,
        string accessKey,
        string secretKey)
        => new AmazonS3Client(accessKey, secretKey, new AmazonS3Config
        {
            ServiceURL = serviceUrl,
            ForcePathStyle = true
        });
}