using AutoMapper;
using VOID.APP.Models.Chat;
using VOID.Shared.Contracts.DTOs.Chats;

namespace VOID.APP.Services.Mappers;

public class ChatMappingProfile : Profile
{
    public ChatMappingProfile()
    {
        CreateMap<ChatDto, ChatModel>();
        CreateMap<FullChatDto, FullChatModel>();
    }
}