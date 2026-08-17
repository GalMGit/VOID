using AutoMapper;
using AutoMapper.Execution;
using VOID.Application.Abstractions.IServices.IMediaServices;
using VOID.Domain.Models.Users;
using VOID.Shared.Contracts.DTOs.Users;
using VOID.Shared.Contracts.DTOs.Users.Accounts;

namespace VOID.Application.Mappers.Users;

public sealed class AuthUserAvatarMappingResolver(
    IMediaUrlService mediaUrlService) : IValueResolver<User, UserAuthDto, string?>
{
    public string? Resolve(
        User source,
        UserAuthDto destination,
        string? destMember,
        ResolutionContext context)
        => string.IsNullOrWhiteSpace(source.AvatarUrl)
            ? null
            : mediaUrlService.GetAvatarUrl(source.AvatarUrl);
}

public sealed class SearchUserAvatarMappingResolver(
    IMediaUrlService mediaUrlService) : IValueResolver<User, SearchUserDto, string?>
{
    public string? Resolve(
        User source,
        SearchUserDto destination,
        string? destMember,
        ResolutionContext context)
        => string.IsNullOrWhiteSpace(source.AvatarUrl)
            ? null
            : mediaUrlService.GetAvatarUrl(source.AvatarUrl);
}