using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using VOID.Application.Abstractions.IRepositories.IGroupRepositories;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Groups.Events.Created;
using VOID.Domain.Enums.Roles.Group;
using VOID.Domain.Models.Groups;
using VOID.Shared.Contracts.DTOs.Groups;
using Wolverine;

namespace VOID.Application.UseCases.Groups.Commands.Create;

public sealed class CreateGroupCommandHandler(
    IGroupRepository groupRepository,
    IMapper mapper,
    IMessageBus bus)
{
    public async Task<GroupDto> Handle(
        CreateGroupCommand request,
        CancellationToken ct)
    {
        var countByUser = await groupRepository.GetTotalCountOwnedAsync(
            request.UserId, ct);

        if (countByUser >= 3)
            throw new ValidationException("Максимум групп для создания - 3");

        var group = new GroupChat
        {
            Id = Guid.NewGuid(),
            ChatName = request.Dto.GroupName,
            Description = null,
            ImageUrl = null,
            CreatedAt = DateTime.UtcNow,
            OwnerId = request.UserId
        };

        group.GroupMembers.Add(new GroupMember
        {
            Id = Guid.NewGuid(),
            MemberId = request.UserId,
            GroupRole = GroupRole.Owner,
            CreatedAt = group.CreatedAt,
            GroupId = group.Id
        });

        var createdGroup = await groupRepository.CreateAsync(
            group, ct);
        
        var groupDto = mapper.Map<GroupDto>(createdGroup);

        await bus.PublishAsync(
            new GroupCreatedEvent(
                groupDto, 
                request.UserId));

        return groupDto;
    }
}