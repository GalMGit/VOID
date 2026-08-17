using System.Security.Claims;
using VOID.API.Extensions;
using VOID.Application.UseCases.Chats.Commands.Create;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using VOID.Shared.Contracts.DTOs.Chats;
using Wolverine;

namespace VOID.API.Endpoints.Chats;

public sealed class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("chats", async (
            CreateChatDto dto,
            IMessageBus command,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var result = await command.InvokeAsync<ChatDto>(
                new CreateChatCommand(
                    dto,
                    user.GetUserId()), ct);

            return Results.Ok(result);
        })
        .WithTags(Tags.Chats)
        .RequireAuthorization();
    }
}
