namespace VOID.Infrastructure.Storage;

public sealed class ApiOptions
{
    public string BaseUrl { get; init; } = null!;
    public string ApiPrefix { get; init; } = null!;
}