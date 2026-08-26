using System;
using System.Threading;
using System.Threading.Tasks;
using VOID.Application.Abstractions.IRepositories.IUserRepositories;
using VOID.Application.Abstractions.IServices.IAuthServices;
using VOID.Application.Abstractions.IServices.ICacheServices;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Auth.Events;
using VOID.Domain.Enums.Roles.App;
using VOID.Domain.Models.Users;
using VOID.Shared.Contracts.DTOs.Auth.Register;
using Wolverine;

namespace VOID.Application.UseCases.Auth.Commands.Register;

public sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    ICacheService cacheService,
    IPasswordHasher passwordHasher,
    IMessageBus bus)
{
    public async Task<RegisterDto> Handle(
        RegisterUserCommand request, 
        CancellationToken ct)
    {
        if (await userRepository.UsernameExistsAsync(
                request.Dto.Username, ct))
            throw new ConflictException("Пользователь с таким username уже существует");

        if (await userRepository.EmailExistsAsync(
                request.Dto.Email, ct))
            throw new ConflictException("Пользователь с таким email уже существует");

        var cacheKey = $"temp_user:{request.Dto.Email}";

        if (await cacheService.ExistsAsync(
                cacheKey, ct))
            throw new ConflictException("На этот email уже отправлен код подтверждения (10 мин)");

        var confirmationCode = new Random()
            .Next(10000, 99999)
            .ToString();

        var tempUser = new TempUser
        {
            Id = Guid.NewGuid(),
            Username = request.Dto.Username,
            Email = request.Dto.Email,
            Name = request.Dto.Username,
            PasswordHash = passwordHasher.GenerateHash(request.Dto.Password),
            CreatedAt = DateTime.UtcNow,
            Role = AppRole.User,
            ConfirmationCode = confirmationCode,
            CodeExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };

        await cacheService.SetAsync(
            cacheKey,
            tempUser,
            TimeSpan.FromMinutes(10),
            ct);

        await bus.PublishAsync(
            new UserStartRegistrationEvent(
                tempUser.Id,
                tempUser.Email,
                tempUser.Username,
                tempUser.ConfirmationCode));
        
        return new RegisterDto { Email = request.Dto.Email };
    }
}