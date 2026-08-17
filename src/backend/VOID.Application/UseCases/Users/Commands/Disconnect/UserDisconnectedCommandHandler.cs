using System.Threading.Tasks;
using VOID.Application.Abstractions.IRepositories.IUserRepositories;
using VOID.Application.UseCases.Users.Events.Connections;
using Wolverine;

namespace VOID.Application.UseCases.Users.Commands.Disconnect;

public sealed class UserDisconnectedCommandHandler(
    IUserRepository userRepository,
    IMessageBus bus)
{
    public async Task Handle(UserDisconnectedCommand request)
    {
        await userRepository.OnlineStatusChangeAsync(
            request.UserId, 
            false);

        await userRepository.ChangeUserLastSeenAsync(
            request.UserId);

        await bus.PublishAsync(
            new UserStatusChangedEvent(
                request.UserId, 
                false));
    }
}