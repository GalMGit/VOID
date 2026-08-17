using System;
using AutoMapper;
using VOID.Domain.Models.Groups;
using VOID.Shared.Contracts.DTOs.Groups;

namespace VOID.Application.Mappers.Groups;

public class GroupMappingProfile : Profile
{
    public GroupMappingProfile()
    {
        CreateMap<GroupChat, GroupDto>()
            .ForMember(x => x.ImageUrl, s => s.MapFrom<ImageMappingResolver>());
        
        CreateMap<GroupMember, GroupMemberDto>()
            .ForMember(x => x.AvatarUrl, s => s.MapFrom<MemberAvatarMappingResolver>())
            .ForMember(x => x.Username, s => s.MapFrom(x => x.Member.Username));

        CreateMap<GroupChat, FullGroupDto>()
            .ForMember(dest => dest.Members,
                opt => opt.MapFrom(src => src.GroupMembers))
            .ForMember(x => x.ImageUrl, s => s.MapFrom<ImageFullMappingResolver>());
    }
}
