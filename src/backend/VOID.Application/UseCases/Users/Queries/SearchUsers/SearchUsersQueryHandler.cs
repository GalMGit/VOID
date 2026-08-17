using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using VOID.Application.Abstractions.IRepositories.IUserRepositories;
using VOID.Shared.Contracts.DTOs.Users;

namespace VOID.Application.UseCases.Users.Queries.SearchUsers;

public sealed class SearchUsersQueryHandler(
    IUserRepository userRepository,
    IMapper mapper) 
{
    public async Task<List<SearchUserDto>> HandleAsync(
        SearchUsersQuery request, 
        CancellationToken ct)
    {
        var users = await userRepository.SearchAsync(
            request.Username, 
            request.UserId, 
            ct);

        return mapper.Map<List<SearchUserDto>>(users);
    }
}