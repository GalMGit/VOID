using System;
using AutoMapper;
using VOID.Domain.Enums.Types.Chat;
using VOID.Domain.Models.Messages;
using VOID.Shared.Contracts.DTOs.Messages;

namespace VOID.Application.Mappers.Messages;

public class MessageMappingProfile : Profile
{
    public MessageMappingProfile()
    {
        CreateMap<Message, MessageDto>()
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Sender.Name))
            .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom<AuthorAvatarUrlResolver>())
            .ForMember(dest => dest.MediaUrl,
                opt => opt.MapFrom<MessageMediaUrlResolver>())
            .ForMember(dest => dest.ThumbnailUrl,
                opt => opt.MapFrom<MessageThumbnailUrlResolver>())
            .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src =>
                src.ChatType == ChatType.Private 
                    ? src.ChatId!.Value 
                    : src.GroupChatId!.Value))
            .AfterMap((src, dest, context) =>
            {
                var currentUserId = (Guid)context.Items["CurrentUserId"];
                dest.IsMine = src.SenderId == currentUserId;
            });
    }
}
