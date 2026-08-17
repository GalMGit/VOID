using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using VOID.API.Extensions;
using VOID.Application.Abstractions.IServices.IMediaServices;
using VOID.Application.UseCases.Messages.Commands.Create;
using VOID.API.EndpointsConfig;
using VOID.API.EndpointsConfig.EnpdpointTags;
using VOID.API.Filters;
using VOID.Shared.Contracts.DTOs.Messages;
using Wolverine;

namespace VOID.API.Endpoints.Messages;

public sealed class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("messages", async (
            [FromForm] CreateMessageRequest dto,
            IMessageBus command,
            ClaimsPrincipal user, 
            CancellationToken ct) =>
            {
                var upload = dto.Media is null 
                    ? null 
                    : new UploadFile 
                    {
                        FileName = dto.Media.FileName,
                        ContentType = dto.Media.ContentType,
                        Length = dto.Media.Length,
                        Stream = dto.Media.OpenReadStream() 
                    };
                
                await using (upload)
                {
                    var result = await command.InvokeAsync<MessageDto>(
                        new CreateMessageCommand(
                            dto, 
                            user.GetUserId(), 
                            upload), ct);
                    
                    return Results.Ok(result);
                }
            })
            .WithTags(Tags.Messages)
            .RequireAuthorization()
            .DisableAntiforgery()
            .AddEndpointFilter<FluentValidationFilter<CreateMessageRequest>>();
    }
}

public class CreateMessageRequest : CreateMessageDto
{
    public IFormFile? Media { get; init; }
}
