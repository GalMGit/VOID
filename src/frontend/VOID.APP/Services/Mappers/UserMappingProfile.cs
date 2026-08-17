using AutoMapper;
using VOID.APP.Models.User;
using VOID.Shared.Contracts.DTOs.Users.Accounts;

namespace VOID.APP.Services.Mappers;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<UserAuthDto, UserAuthModel>();
    }
}
