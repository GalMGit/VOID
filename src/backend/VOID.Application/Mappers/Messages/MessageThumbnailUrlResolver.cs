using AutoMapper;
using VOID.Application.Abstractions.IServices.IMediaServices;
using VOID.Domain.Models.Messages;
using VOID.Shared.Contracts.DTOs.Messages;

namespace VOID.Application.Mappers.Messages;

public sealed class MessageThumbnailUrlResolver(
    IMediaUrlService mediaUrlService)
    : IValueResolver<Message, MessageDto, string?>
{
    public string? Resolve(
        Message source,
        MessageDto destination,
        string? destMember,
        ResolutionContext context)
    {
        if (string.IsNullOrWhiteSpace(source.ThumbnailUrl))
            return null;

        return mediaUrlService.GetMessageThumbnailUrl(source.Id);
    }
}