using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using VOID.API.Extensions;
using VOID.Application.UseCases.Messages.Commands.DeleteMessages;
using VOID.Shared.Contracts.DTOs.Messages;
using Wolverine;

namespace VOID.API.Endpoints.Messages;

public sealed class DeleteMessages : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("messages", async (
                [FromBody] DeleteMessagesDto dto,
                ClaimsPrincipal user,
                [FromServices] IMessageBus command,
                CancellationToken ct) =>
            {
                await command.InvokeAsync(
                    new DeleteMessagesCommand(
                        dto,
                        user.GetUserId()), ct);
            })
            .WithTags(Tags.Messages)
            .RequireAuthorization();
    }
}