using System.Threading.Tasks;
using VOID.Application.Abstractions.IRepositories.IUserRepositories;
using VOID.Application.UseCases.Users.Events.Connections;
using Wolverine;

namespace VOID.Application.UseCases.Users.Commands.Connect;

public sealed class UserConnectedCommandHandler(
    IUserRepository userRepository,
    IMessageBus bus)
{
    public async Task Handle(UserConnectedCommand request)
    {
        await userRepository.OnlineStatusChangeAsync(
            request.UserId, 
            true);

        await bus.PublishAsync(
            new UserStatusChangedEvent(
                request.UserId, 
                true));
    }
}