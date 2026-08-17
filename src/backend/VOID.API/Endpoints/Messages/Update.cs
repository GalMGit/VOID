using System.Security.Claims;
using VOID.API.Extensions;
using VOID.Application.UseCases.Messages.Commands.Update;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using VOID.Shared.Contracts.DTOs.Messages;
using Wolverine;

namespace VOID.API.Endpoints.Messages;

public class Update : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("messages/{messageId}", async (
            Guid messageId,
            IMessageBus command,
            UpdateMessageDto dto,
            ClaimsPrincipal user, 
            CancellationToken ct) =>
        {
            await command.InvokeAsync(
                new UpdateMessageCommand(
                    dto, 
                    messageId, 
                    user.GetUserId()), ct);
            
            return Results.Ok();
        })
        .WithTags(Tags.Messages)
        .RequireAuthorization();
    }
}
