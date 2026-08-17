using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using VOID.Application.Abstractions.IRepositories.IUserRepositories;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Users.Events.Profile;
using VOID.Shared.Contracts.DTOs.Users.Accounts;
using Wolverine;

namespace VOID.Application.UseCases.Users.Commands.Update;

public class UpdateUserCommandHandler(
    IUserRepository userRepository,
    IMapper mapper,
    IMessageBus bus)
{
    public async Task<UserAuthDto> Handle(
        UpdateUserCommand request, 
        CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(
                       request.UserId, ct)
                   ?? throw new NotFoundException("Пользователь не найден");
        
        if (request.Dto.Name == user.Name 
            && request.Dto.AboutMe == user.AboutMe)
            throw new ValidationException("Данные должны отличаться");

        if (!string.IsNullOrWhiteSpace(request.Dto.Name) 
            && request.Dto.Name != user.Name)
        {
            user.Name = request.Dto.Name;
        }
        else if (string.IsNullOrWhiteSpace(request.Dto.Name))
        {
            user.Name = user.Username;
        }

        if (!string.IsNullOrWhiteSpace(request.Dto.AboutMe) 
            && request.Dto.AboutMe != user.AboutMe)
        {
            user.AboutMe = request.Dto.AboutMe;
        }
        else if (string.IsNullOrWhiteSpace(request.Dto.AboutMe))
        {
            user.AboutMe = null;
        }
        
        var updatedUser = await userRepository.UpdateAsync(
            user, ct);
        
        var userDto = mapper.Map<UserAuthDto>(updatedUser);

        await bus.PublishAsync(
            new UserUpdatedEvent(
                request.UserId, 
                request.Dto.Name!));
        
        return userDto;
    }
}