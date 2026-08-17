using AutoMapper;
using VOID.APP.Models.Group;
using VOID.Shared.Contracts.DTOs.Groups;

namespace VOID.APP.Services.Mappers;

public class GroupMappingProfile : Profile
{
    public GroupMappingProfile()
    {
        CreateMap<GroupDto, GroupModel>();
        CreateMap<GroupMemberDto, GroupMemberModel>();
        CreateMap<FullGroupDto, FullGroupModel>();
    }
}