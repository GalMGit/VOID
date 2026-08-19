using VOID.API.EndpointsConfig;

namespace VOID.API.Endpoints.Tests;

public class Test : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("test", () => "123").AllowAnonymous();
    }
}