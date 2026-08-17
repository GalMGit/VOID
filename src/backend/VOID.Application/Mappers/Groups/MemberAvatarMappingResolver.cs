using AutoMapper;
using AutoMapper.Execution;
using VOID.Application.Abstractions.IServices.IMediaServices;
using VOID.Domain.Models.Groups;
using VOID.Shared.Contracts.DTOs.Groups;

namespace VOID.Application.Mappers.Groups;

public sealed class MemberAvatarMappingResolver(
    IMediaUrlService mediaUrlService) 
    : IValueResolver<GroupMember, GroupMemberDto, string?>
{
    public string? Resolve(
        GroupMember source,
        GroupMemberDto destination,
        string? destMember,
        ResolutionContext context)
        => string.IsNullOrWhiteSpace(source.Member.AvatarUrl)
            ? null
            : mediaUrlService.GetAvatarUrl(source.Member.AvatarUrl);
}