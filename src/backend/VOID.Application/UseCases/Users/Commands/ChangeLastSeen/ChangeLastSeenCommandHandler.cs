using System.Threading;
using System.Threading.Tasks;
using VOID.Application.Abstractions.IRepositories.IUserRepositories;

namespace VOID.Application.UseCases.Users.Commands.ChangeLastSeen;

public class ChangeLastSeenCommandHandler(
    IUserRepository userRepository) 
{
    public async Task HandleAsync(
        ChangeLastSeenCommand request, 
        CancellationToken ct)
    {
        await userRepository.ChangeUserLastSeenAsync(
            request.UserId, 
            ct);
    }
}