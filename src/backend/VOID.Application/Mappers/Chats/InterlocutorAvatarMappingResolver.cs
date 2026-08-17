using System;
using AutoMapper;
using VOID.Domain.Models.Chats;
using VOID.Shared.Contracts.DTOs.Chats;
using System.Linq;
using VOID.Application.Abstractions.IServices.IMediaServices;

namespace VOID.Application.Mappers.Chats;

public sealed class InterlocutorAvatarMappingResolver(
    IMediaUrlService mediaUrlService) 
    : IValueResolver<Chat, ChatDto, string?>
{
    public string? Resolve(
        Chat source,
        ChatDto destination, 
        string? destMember, 
        ResolutionContext context)
    {
        var currentUserId = (Guid)context.Items["CurrentUserId"];
        var other = source.Interlocutors
            .FirstOrDefault(x => x.UserId != currentUserId);
        
        return other?.User.AvatarUrl == null 
            ? null 
            : mediaUrlService.GetAvatarUrl(other.User.AvatarUrl);
    }
}

public sealed class InterlocutorFullChatAvatarMappingResolver(
    IMediaUrlService mediaUrlService) 
    : IValueResolver<Chat, FullChatDto, string?>
{
    public string? Resolve(
        Chat source,
        FullChatDto destination, 
        string? destMember, 
        ResolutionContext context)
    {
        var currentUserId = (Guid)context.Items["CurrentUserId"];
        var other = source.Interlocutors
            .FirstOrDefault(x => x.UserId != currentUserId);
        
        return other?.User.AvatarUrl == null 
            ? null 
            : mediaUrlService.GetAvatarUrl(other.User.AvatarUrl);
    }
}
