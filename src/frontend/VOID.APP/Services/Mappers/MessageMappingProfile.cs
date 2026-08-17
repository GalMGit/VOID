using AutoMapper;
using VOID.APP.Models.Messages;
using VOID.Shared.Contracts.DTOs.Messages;

namespace VOID.APP.Services.Mappers;

public class MessageMappingProfile : Profile
{
    public MessageMappingProfile()
    {
        CreateMap<MessageDto, MessageModel>();
    }
}