using AutoMapper;
using AutoMapper.Execution;
using VOID.Application.Abstractions.IServices.IMediaServices;
using VOID.Domain.Models.Messages;
using VOID.Shared.Contracts.DTOs.Messages;

namespace VOID.Application.Mappers.Messages;

public sealed class MessageMediaUrlResolver(
    IMediaUrlService mediaUrlService) 
    : IValueResolver<Message, MessageDto, string?>
{
    public string? Resolve(
        Message source,
        MessageDto destination,
        string? destMember,
        ResolutionContext context)
    {
        if (string.IsNullOrWhiteSpace(source.MediaUrl))
            return null;

        return mediaUrlService.GetMessageMediaUrl(source.Id);
    }
}