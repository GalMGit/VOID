using System.Threading;
using System.Threading.Tasks;
using VOID.Application.Abstractions.IRepositories.IGroupRepositories;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Groups.Events.Deleted;
using Wolverine;

namespace VOID.Application.UseCases.Groups.Commands.Delete;

public sealed class DeleteGroupCommandHandler(
    IGroupRepository groupRepository,
    IMessageBus bus)
{
    public async Task Handle(
        DeleteGroupCommand request, 
        CancellationToken ct)
    {
        if (!await groupRepository.ExistsAsync(
                request.GroupId, ct))
            throw new NotFoundException("Группа не найдена");

        if (!await groupRepository.IsOwnerAsync(
                request.GroupId,
                request.UserId, ct))
            throw new ForbiddenException();

        await groupRepository.DeleteAsync(
            request.GroupId, ct);

        await bus.PublishAsync(
            new GroupDeletedEvent(
                request.GroupId));
    }
}