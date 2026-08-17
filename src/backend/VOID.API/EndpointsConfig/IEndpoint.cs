namespace VOID.API.EndpointsConfig;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}