using System;
using AutoMapper;
using VOID.Domain.Models.Users;
using VOID.Shared.Contracts.DTOs.Auth.Register;
using VOID.Shared.Contracts.DTOs.Users;
using VOID.Shared.Contracts.DTOs.Users.Accounts;
using VOID.Shared.Contracts.DTOs.Users.Avatars;

namespace VOID.Application.Mappers.Users;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, RegisterDto>()
            .ForMember(x => x.Email, x => x.MapFrom(u => u.Email));

        CreateMap<User, AvatarDto>();

        CreateMap<User, SearchUserDto>().ForMember(x => x.AvatarUrl, s =>
            s.MapFrom<SearchUserAvatarMappingResolver>());

        CreateMap<User, UserAuthDto>().ForMember(x => x.AvatarUrl, s => 
            s.MapFrom<AuthUserAvatarMappingResolver>());
    }
}
