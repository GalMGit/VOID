using VOID.API.EndpointsConfig;

namespace VOID.API.Endpoints.Tests;

public sealed class Test : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("test", () =>
                "Endpoint has been tested")
            .AllowAnonymous();
    }
}