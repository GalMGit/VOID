using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using VOID.Application.Abstractions.IRepositories.IGroupRepositories;
using VOID.Application.Abstractions.IRepositories.IUserRepositories;
using VOID.Application.Exceptions;
using VOID.Shared.Contracts.DTOs.Users;

namespace VOID.Application.UseCases.Groups.Queries.SearchUsersForGroup;

public sealed class SearchUsersForGroupQueryHandler(
    IGroupRepository groupRepository, 
    IUserRepository userRepository,
    IMapper mapper)
{
    public async Task<List<SearchUserDto>> Handle(
        SearchUsersForGroupQuery request, 
        CancellationToken ct)
    {
        var isMember = await groupRepository.IsMemberAsync(
                request.GroupId, 
                request.UserId, ct);

        if (!isMember)
            throw new ForbiddenException();

        var users = await userRepository.SearchUsersForGroupAsync(
                request.SearchTerm, 
                request.UserId, 
                request.GroupId, ct);
        
        return mapper.Map<List<SearchUserDto>>(users);
    }
}