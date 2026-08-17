using System.Security.Claims;
using VOID.API.Extensions;
using VOID.Application.UseCases.Messages.Commands.Delete;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using Wolverine;

namespace VOID.API.Endpoints.Messages;

public sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("messages/{messageId:guid}", async (
                Guid messageId,
                IMessageBus command,
                ClaimsPrincipal user, 
                CancellationToken ct) =>
            {
                await command.InvokeAsync(
                        new DeleteMessageCommand(
                            messageId, 
                            user.GetUserId()), ct);
                
                return Results.Ok();
            })
            .WithTags(Tags.Messages)
            .RequireAuthorization();
    }
}