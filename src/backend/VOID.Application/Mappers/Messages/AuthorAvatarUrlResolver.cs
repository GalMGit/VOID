using AutoMapper;
using VOID.Application.Abstractions.IServices.IMediaServices;
using VOID.Domain.Models.Messages;
using VOID.Domain.Models.Users;
using VOID.Shared.Contracts.DTOs.Messages;

namespace VOID.Application.Mappers.Messages;

public sealed class AuthorAvatarUrlResolver(
    IMediaUrlService mediaUrlService) : IValueResolver<Message, MessageDto, string?>
{
    public string? Resolve(
        Message source,
        MessageDto destination, 
        string? destMember, 
        ResolutionContext context)
        => string.IsNullOrWhiteSpace(source.Sender.AvatarUrl) 
            ? null 
            : mediaUrlService.GetAvatarUrl(source.Sender.AvatarUrl);
    
}