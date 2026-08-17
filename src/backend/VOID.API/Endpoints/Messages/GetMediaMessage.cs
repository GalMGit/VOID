using System.Security.Claims;
using VOID.API.Extensions;
using VOID.Application.UseCases.Messages.Queries.GetMedia;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using VOID.Shared.Contracts.DTOs.Messages;
using Wolverine;

namespace VOID.API.Endpoints.Messages;

public sealed class GetMediaMessage : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("messages/{messageId:guid}/media", async (
                Guid messageId,
                ClaimsPrincipal user,
                IMessageBus query,
                CancellationToken ct) =>
            {
                var result = await query.InvokeAsync<MessageMediaResult>(
                    new GetMediaMessageQuery(
                        user.GetUserId(),
                        messageId), ct);
                
                var isVideo = result.ContentType.StartsWith("video/");

                if (isVideo)
                    return Results.Ok(new
                    {
                        url = result.Url
                    });

                return Results.Redirect(
                    result.Url, 
                    permanent: false);
            })
            .WithTags(Tags.Messages)
            .RequireAuthorization();
    }
}