using System;
using System.Threading;
using System.Threading.Tasks;
using VOID.Application.Abstractions.IRepositories.IGroupRepositories;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Groups.Events.MemberDeleted;
using Wolverine;

namespace VOID.Application.UseCases.Groups.Commands.DeleteMembers;

public sealed class DeleteMemberFromGroupCommandHandler(
    IGroupRepository groupRepository)
{
    public async Task Handle(
        DeleteMemberFromGroupCommand request, 
        CancellationToken ct,
        IMessageBus bus)
    {
        if (!await groupRepository.ExistsAsync(request.GroupId, ct))
            throw new NotFoundException("Группа не найдена");

        if (!await groupRepository.IsMemberAsync(
                request.GroupId, 
                request.MemberId ,ct))
            throw new NotFoundException("Участник не найден");

        if (!await groupRepository.IsOwnerAsync(
                request.GroupId, 
                request.UserId, ct))
            throw new UnauthorizedAccessException();

        if(await groupRepository.IsOwnerAsync(
               request.GroupId, 
               request.MemberId, ct))
            throw new ValidationException("Создатель не может удалить себя из группы");
        
        await groupRepository.DeleteMemberAsync(
            request.GroupId, 
            request.MemberId, ct);

        await bus.PublishAsync(
            new MemberDeletedEvent(
                request.GroupId, 
                request.MemberId,
                request.UserId));
    }
}