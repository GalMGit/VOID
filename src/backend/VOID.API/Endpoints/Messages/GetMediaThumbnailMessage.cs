using System.Security.Claims;
using VOID.API.Extensions;
using VOID.Application.UseCases.Messages.Queries.GetThumbnailMedia;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using VOID.Shared.Contracts.DTOs.Messages;
using Wolverine;

namespace VOID.API.Endpoints.Messages;

public sealed class GetMediaThumbnailMessage : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("messages/{messageId:guid}/thumbnail", async (
                Guid messageId,
                ClaimsPrincipal user,
                IMessageBus query,
                CancellationToken ct) =>
            {
                var result = await query.InvokeAsync<MessageThumbnailResult>(
                    new GetThumbnailMediaQuery(
                        user.GetUserId(),
                        messageId), ct);

                return Results.Redirect(
                    result.ThumbnailUrl, 
                    permanent: false);
            })
            .WithTags(Tags.Messages)
            .RequireAuthorization();
    }
}