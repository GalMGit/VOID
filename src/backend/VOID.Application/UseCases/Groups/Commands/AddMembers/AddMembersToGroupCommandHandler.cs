using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using VOID.Application.Abstractions.IRepositories.IChatRepositories;
using VOID.Application.Abstractions.IRepositories.IGroupRepositories;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Groups.Events.MembersAdded;
using VOID.Domain.Enums.Roles.Group;
using VOID.Domain.Models.Groups;
using VOID.Shared.Contracts.DTOs.Groups;
using Wolverine;

namespace VOID.Application.UseCases.Groups.Commands.AddMembers;

public sealed class AddMembersToGroupCommandHandler(
    IGroupRepository groupRepository,
    IChatRepository chatRepository,
    IMessageBus bus,
    IMapper mapper)
{
    public async Task<List<GroupMemberDto>> Handle(
        AddMembersToGroupCommand request,
        CancellationToken ct)
    {
        if (!await groupRepository.ExistsAsync(
                request.GroupId, ct))
            throw new NotFoundException("Группа не найдена");

        if (!await groupRepository.IsMemberAsync(
                request.GroupId,
                request.UserId, ct))
            throw new ForbiddenException();

        var userIdsToAdd = request.Dto.Members
            .ToList();

        var existingMemberIds = await groupRepository.GetExistingMemberIdsAsync(
                request.GroupId,
                userIdsToAdd, ct);

        var usersWithChats = await chatRepository.GetUsersWithChatsAsync(
                request.UserId,
                userIdsToAdd, ct);

        var membersToAdd = new List<GroupMember>();

        foreach (var memberDto in request.Dto.Members)
        {
            if (existingMemberIds.Contains(memberDto))
                throw new ConflictException(
                    $"Пользователь {memberDto} уже существует в группе");

            if (!usersWithChats.Contains(memberDto))
                throw new ConflictException(
                    $"У вас нет личного чата с {memberDto}");

            membersToAdd.Add(new GroupMember
            {
                Id = Guid.NewGuid(),
                GroupId = request.GroupId,
                MemberId = memberDto,
                GroupRole = GroupRole.Member,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (membersToAdd.Count != 0)
            await groupRepository.AddMembersRangeAsync(
                membersToAdd,
                ct);

        var addedMemberIds = membersToAdd
            .Select(x => x.MemberId)
            .ToList();

        var addedMembersWithDetails = await groupRepository.GetMembersWithDetailsAsync(
                request.GroupId,
                addedMemberIds, ct);

        var mapperMembers = mapper.Map<List<GroupMemberDto>>(addedMembersWithDetails);
        
        var group = await groupRepository.GetByIdAsync(
            request.GroupId, ct);
        
        var groupDto = mapper.Map<GroupDto>(group);

        await bus.PublishAsync(
            new MembersAddedEvent(
                groupDto, 
                addedMemberIds, 
                request.UserId));
        
        return mapperMembers;
    }
}