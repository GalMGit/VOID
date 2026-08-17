using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using VOID.Application.Abstractions.IRepositories.IGroupRepositories;
using VOID.Application.Abstractions.IRepositories.IMessageRepositories;
using VOID.Application.Exceptions;
using VOID.Shared.Contracts.DTOs.Groups;

namespace VOID.Application.UseCases.Groups.Queries.GetById;

public sealed class GetGroupByIdQueryHandler(
    IGroupRepository groupRepository,
    IMessageRepository messageRepository, 
    IMapper mapper) 
{
    public async Task<FullGroupDto> Handle(
        GetGroupByIdQuery request, 
        CancellationToken ct)
    {
        var group = await groupRepository.GetByIdAsync(
                        request.GroupId, ct) 
                    ?? throw new NotFoundException("Группа не найдена");

        if (group.GroupMembers
            .All(x => x.MemberId != request.UserId))
            throw new ForbiddenException();

        var messageCount = await messageRepository.GetTotalCountByGroupAsync(
            group.Id, ct);
        
        var mappedGroup = mapper.Map<FullGroupDto>(group);

        mappedGroup.MessageCount = messageCount;
        
        return mappedGroup;
    }
}