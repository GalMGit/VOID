using System.Threading;
using System.Threading.Tasks;
using VOID.Application.Abstractions.IRepositories.IGroupRepositories;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Groups.Events.Leaved;
using Wolverine;

namespace VOID.Application.UseCases.Groups.Commands.LeaveFromGroup;

public sealed class LeaveFromGroupCommandHandler(
    IGroupRepository groupRepository,
    IMessageBus bus)
{
    public async Task Handle(
        LeaveFromGroupCommand request, 
        CancellationToken ct)
    {
        if (!await groupRepository.ExistsAsync(
                request.GroupId, ct))
            throw new NotFoundException("Группа не найдена");

        if (!await groupRepository.IsMemberAsync(
                request.GroupId, 
                request.UserId, ct))
            throw new NotFoundException("Участник не найден");

        if (await groupRepository.IsOwnerAsync(
                request.GroupId, 
                request.UserId, ct))
            throw new ValidationException("Создатель не может покинуть группу");

        await groupRepository.DeleteMemberAsync(
            request.GroupId, 
            request.UserId, ct);

        await bus.PublishAsync(
            new LeavedFromGroupEvent(
                request.GroupId,
                request.UserId));
    }
}