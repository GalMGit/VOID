using System;
using System.Linq;
using AutoMapper;
using VOID.Domain.Models.Chats;
using VOID.Shared.Contracts.DTOs.Chats;

namespace VOID.Application.Mappers.Chats;

public class ChatMappingProfile : Profile
{
    public ChatMappingProfile()
    {
        CreateMap<Chat, ChatDto>()
            .ForMember(dest => dest.ImageUrl, 
                opt => opt.MapFrom<InterlocutorAvatarMappingResolver>())
            .AfterMap((src, dest, context) =>
            {
                var currentUserId = (Guid)context.Items["CurrentUserId"];
                var other = src.Interlocutors
                    .FirstOrDefault(x => x.UserId != currentUserId);
                if (other is null) return;

                dest.ChatName = other.User.Name;
                dest.LastMessage = src.LastMessage;
                dest.LastMessageDate = src.LastMessageDate;
                dest.InterlocutorId = other.UserId;
                dest.InterlocutorOnline = other.User.IsOnline;
            });

        CreateMap<ChatInterlocutor, ChatInterlocutorDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.User.Id))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Name))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.LastSeen, opt => opt.MapFrom(src => src.User.LastSeen));

        CreateMap<Chat, FullChatDto>()
            .ForMember(dest => dest.ImageUrl, 
                opt => opt.MapFrom<InterlocutorFullChatAvatarMappingResolver>())
            .AfterMap((src, dest, context) =>
            {
                var currentUserId = (Guid)context.Items["CurrentUserId"];
                var other = src.Interlocutors.FirstOrDefault(x => x.UserId != currentUserId);
                if (other is null) return;

                dest.ChatName = other.User.Name;
                dest.InterlocutorId = other.UserId;
                dest.InterlocutorLastSeen = other.User.LastSeen;
                dest.InterlocutorAboutMe = other.User.AboutMe;
                dest.InterlocutorOnline = other.User.IsOnline;
                dest.InterlocutorUsername = other.User.Username;
            });
    }
}