using System;
using System.Threading;
using System.Threading.Tasks;
using VOID.Application.Abstractions.IRepositories.IUserRepositories;
using VOID.Application.Abstractions.IServices.ICacheServices;
using VOID.Application.Exceptions;
using VOID.Application.UseCases.Auth.Events;
using VOID.Domain.Models.Users;
using Wolverine;

namespace VOID.Application.UseCases.Auth.Commands.ResetPassword;

public sealed class StartResetPasswordCommandHandler(
    ICacheService cacheService,
    IUserRepository userRepository,
    IMessageBus bus)
{
    public async Task Handle(
        StartResetPasswordCommand request, 
        CancellationToken ct)
    {
        var normalizedEmail = request.Dto.Email
            .Trim()
            .ToLowerInvariant();
        
        if (!await userRepository.EmailExistsAsync(request.Dto.Email, ct))
            return;
        
        var cacheKey = $"temp_resetPassword:{normalizedEmail}";
        
        if (await cacheService.ExistsAsync(
                cacheKey, ct))
            throw new ConflictException("На этот email уже отправлен код подтверждения (5 мин)");
        
        var confirmationCode = new Random()
            .Next(10000, 99999)
            .ToString();

        var tempCode = new TempCode
        {
            CodeExpiresAt = DateTime.UtcNow.AddMinutes(5),
            ConfirmationCode = confirmationCode
        };
        
        await cacheService.SetAsync(
            cacheKey,
            tempCode,
            TimeSpan.FromMinutes(5),
            ct);

        await bus.PublishAsync(
            new SendStartResetPasswordEvent(
                request.Dto.Email, 
                confirmationCode));
    }
}