using System;
using System.Threading;
using System.Threading.Tasks;
using VOID.Application.Abstractions.IRepositories.IUserRepositories;
using VOID.Application.Abstractions.IServices.ICacheServices;
using VOID.Application.Exceptions;
using VOID.Domain.Models.Users;

namespace VOID.Application.UseCases.Auth.Commands.ConfirmEmail;

public class ConfirmEmailCommandHandler(
    IUserRepository userRepository,
    ICacheService cacheService)
{
    public async Task Handle(
        ConfirmEmailCommand request, 
        CancellationToken ct)
    {
        var cacheKey = $"temp_user:{request.Dto.Email}";
        var tempUser = await cacheService.GetAsync<TempUser>(cacheKey, ct);
        if (tempUser is null)
            throw new NotFoundException("Код подтверждения не найден, зарегистрируйтесь заново");

        if (tempUser.ConfirmationCode != request.Dto.Code)
            throw new ValidationException("Неверный код подтверждения");

        if (tempUser.CodeExpiresAt < DateTime.UtcNow)
            throw new ValidationException("Срок действия кода истек, зарегистрируйтесь заново");

        if (await userRepository.EmailExistsAsync(
                tempUser.Email, ct))
            throw new ConflictException("Пользователь с таким email уже существует");

        if (await userRepository.UsernameExistsAsync(
                tempUser.Username, ct))
            throw new ConflictException("Пользователь с таким username уже существует");

        var user = new User
        {
            Id = tempUser.Id,
            CreatedAt = tempUser.CreatedAt,
            Name = tempUser.Name,
            Username = tempUser.Username,
            Email = tempUser.Email,
            LastSeen = DateTime.UtcNow,
            EmailConfirmed = true,
            AppRole = tempUser.Role,
            PasswordHash = tempUser.PasswordHash
        };
        await userRepository.CreateAsync(
            user, ct);
        
        await cacheService.RemoveAsync(
            cacheKey, ct);
    }
}