using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace VOID.Application.Abstractions.IServices.IMediaServices;

public interface IPrivateStorage
{
    Task UploadAsync(
        string key,
        Stream stream,
        string contentType,
        CancellationToken ct = default);

    Task DeleteAsync(
        string key,
        CancellationToken ct = default);

    Task DeletePrefixAsync(
        string prefix,
        CancellationToken ct = default);
    
    string GetPresignedUrl(string key, TimeSpan lifetime);
}