using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using VOID.Application.Abstractions.IRepositories.IUserRepositories;
using VOID.Application.Exceptions;
using VOID.Shared.Contracts.DTOs.Users.Accounts;

namespace VOID.Application.UseCases.Users.Queries.GetUserInfo;

public sealed class GetUserInfoQueryHandler(
    IUserRepository userRepository,
    IMapper mapper)
{
    public async Task<UserAuthDto> HandleAsync(
        GetUserInfoQuery request, 
        CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(
                       request.UserId, ct)
                   ?? throw new NotFoundException("Пользователь не найден");

        return mapper.Map<UserAuthDto>(user);
    }
}