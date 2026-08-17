using System.Security.Claims;
using VOID.API.Extensions;
using VOID.Application.UseCases.Messages.Queries.GetAll;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using VOID.Shared.Contracts.DTOs.Messages;
using VOID.Shared.Contracts.DTOs.Paginations;
using VOID.Shared.Contracts.Enums.Chats;
using Wolverine;

namespace VOID.API.Endpoints.Messages;

public sealed class GetByParent : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("messages/parent/{parentId:guid}", async (
            Guid parentId,
            ChatType parentType,
            [AsParameters] PaginationRequest pagination,
            IMessageBus query,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var result = await query.InvokeAsync<PaginatedResult<MessageDto>>(
                    new GetMessagesByParentQuery(
                        parentId, 
                        user.GetUserId(), 
                        parentType, 
                        pagination), ct);

            return Results.Ok(result);
        })
        .WithTags(Tags.Messages)
        .RequireAuthorization();
    }
}
