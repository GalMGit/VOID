namespace VOID.Infrastructure.Storage;

public sealed class PrivateStorageOptions
{
    public string ServiceUrl { get; init; } = null!;
    public string Bucket { get; init; } = null!;
    public string AccessKey { get; init; } = null!;
    public string SecretKey { get; init; } = null!;
}