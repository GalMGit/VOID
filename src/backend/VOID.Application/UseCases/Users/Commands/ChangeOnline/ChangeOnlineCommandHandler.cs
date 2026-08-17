using System.Threading;
using System.Threading.Tasks;
using VOID.Application.Abstractions.IRepositories.IUserRepositories;

namespace VOID.Application.UseCases.Users.Commands.ChangeOnline;

public sealed class ChangeOnlineCommandHandler(
    IUserRepository userRepository) 
{
    public async Task HandleAsync(
        ChangeOnlineCommand request, 
        CancellationToken ct)
    {
        await userRepository.OnlineStatusChangeAsync(
            request.UserId, 
            request.IsOnline, 
            ct);
    }
}