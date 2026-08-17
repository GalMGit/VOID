using AutoMapper;
using VOID.Application.Abstractions.IServices.IMediaServices;
using VOID.Domain.Models.Groups;
using VOID.Shared.Contracts.DTOs.Groups;

namespace VOID.Application.Mappers.Groups;

public sealed class ImageFullMappingResolver(
    IMediaUrlService urlService) 
    : IValueResolver<GroupChat, FullGroupDto, string?>
{
    public string? Resolve(
        GroupChat source, 
        FullGroupDto destination,
        string? destMember,
        ResolutionContext context)
        => string.IsNullOrWhiteSpace(source.ImageUrl)
            ? null
            : urlService.GetAvatarUrl(source.ImageUrl);
}