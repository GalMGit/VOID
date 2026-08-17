using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using VOID.Application.Abstractions.IServices.IMediaServices;

namespace VOID.Infrastructure.Storage;

public abstract class S3ObjectStorage
{
    protected readonly IAmazonS3 Client;
    protected readonly string Bucket;

    protected S3ObjectStorage(
        IAmazonS3 client,
        string bucket)
    {
        Bucket = bucket;
        Client = client;
    }
    
    public virtual async Task UploadAsync(
        string key, 
        Stream stream, 
        string contentType, 
        CancellationToken ct = default)
    {
        if (stream.CanSeek)
            stream.Position = 0;

        var request = new PutObjectRequest
        {
            BucketName = Bucket,
            Key = key,
            InputStream = stream,
            ContentType = contentType,
            UseChunkEncoding = false,
            AutoCloseStream = false
        };
        
        await Client.PutObjectAsync(request, ct);
    }

    public virtual async Task<Stream> DownloadAsync(
        string key, 
        CancellationToken ct = default)
    {
        var response = await Client.GetObjectAsync(
            Bucket,
            key,
            ct);

        return response.ResponseStream;
    }

    public virtual async Task DeleteAsync(
        string key, 
        CancellationToken ct = default)
        => await Client.DeleteObjectAsync(
            Bucket,
            key,
            ct);

    public virtual async Task DeletePrefixAsync(
        string prefix, 
        CancellationToken ct = default)
    {
        string? continuationToken = null;

        do
        {
            var response = await Client.ListObjectsV2Async(
                new ListObjectsV2Request
                {
                    BucketName = Bucket,
                    Prefix = prefix,
                    ContinuationToken = continuationToken
                }, ct);

            if (response.S3Objects.Count > 0)
            {
                var deleteRequest = new DeleteObjectsRequest
                {
                    BucketName = Bucket
                };

                foreach (var obj in response.S3Objects)
                {
                    deleteRequest.AddKey(obj.Key);
                }

                await Client.DeleteObjectsAsync(deleteRequest, ct);
            }

            continuationToken = response.NextContinuationToken;
        } while (continuationToken != null);
    }

    public virtual async Task<bool> ExistsAsync(
        string key, 
        CancellationToken ct = default)
    {
        try
        {
            await Client.GetObjectMetadataAsync(
                Bucket,
                key,
                ct);

            return true;
        }
        catch (AmazonS3Exception ex) 
            when(ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }
}