using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace VOID.Application.Abstractions.IServices.IMediaServices;

public interface IPublicStorage
{
    Task UploadAsync(
        string key,
        Stream stream,
        string contentType,
        CancellationToken ct = default);

    Task DeleteAsync(
        string key,
        CancellationToken ct = default);
    
    string GetPublicUrl(string key);
}