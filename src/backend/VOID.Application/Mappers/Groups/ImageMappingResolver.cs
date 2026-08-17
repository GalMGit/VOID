using System.Text.RegularExpressions;
using AutoMapper;
using AutoMapper.Execution;
using VOID.Application.Abstractions.IServices.IMediaServices;
using VOID.Domain.Models.Groups;
using VOID.Shared.Contracts.DTOs.Groups;

namespace VOID.Application.Mappers.Groups;

public sealed class ImageMappingResolver(
    IMediaUrlService urlService) 
    : IValueResolver<GroupChat, GroupDto, string?>
{
    public string? Resolve(
        GroupChat source, 
        GroupDto destination,
        string? destMember,
        ResolutionContext context)
        => string.IsNullOrWhiteSpace(source.ImageUrl)
            ? null
            : urlService.GetAvatarUrl(source.ImageUrl);
}