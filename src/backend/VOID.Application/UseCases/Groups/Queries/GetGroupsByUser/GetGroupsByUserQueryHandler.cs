using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using VOID.Application.Abstractions.IRepositories.IGroupRepositories;
using VOID.Shared.Contracts.DTOs.Groups;
using VOID.Shared.Contracts.DTOs.Paginations;

namespace VOID.Application.UseCases.Groups.Queries.GetGroupsByUser;

public sealed class GetGroupsByUserQueryHandler(
    IGroupRepository groupRepository,
    IMapper mapper) 
{
    public async Task<PaginatedResult<GroupDto>> Handle(
        GetGroupsByUserQuery request, 
        CancellationToken ct)
    {
        var totalCount = await groupRepository.GetTotalCountByUserAsync(
                request.UserId, ct);
        
        var groups = await groupRepository.GetAllByUserAsync(
                request.UserId, 
                request.Pagination, ct);
        
        var groupsResponse = mapper.Map<List<GroupDto>>(groups);
        
        return new PaginatedResult<GroupDto>(
            groupsResponse,
            totalCount,
            request.Pagination.PageNumber,
            request.Pagination.PageSize
        );
    }
}