using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using VOID.Application.Abstractions.IServices.ICacheServices;
using VOID.Application.Exceptions;
using VOID.Domain.Models.Users;

namespace VOID.Application.UseCases.Auth.Commands.ConfirmResetPassword;

public sealed class ConfirmResetPasswordCommandHandler(
    ICacheService cacheService)
{
    public async Task<string> Handle(
        ConfirmResetPasswordCommand request, 
        CancellationToken ct)
    {
        var normalizedEmail = request.Dto.Email
            .Trim()
            .ToLowerInvariant();

        var cacheKey = $"temp_resetPassword:{normalizedEmail}";
        
        var tempCode = await cacheService.GetAsync<TempCode>(
            cacheKey, ct);
        
        if(tempCode is null)
            throw new NotFoundException(
                "Код подтверждения не найден, отправьте заново");
        
        if (tempCode.ConfirmationCode != request.Dto.Code)
            throw new ValidationException(
                "Неверный код подтверждения");
        
        if (tempCode.CodeExpiresAt < DateTime.UtcNow)
            throw new ValidationException(
                "Срок действия кода истек, зарегистрируйтесь заново");
        
        await cacheService.RemoveAsync(
            cacheKey, ct);

        var resetToken = Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(32));

        var tokenKey = $"reset-password:confirmed:{resetToken}";

        await cacheService.SetAsync(
            tokenKey,
            normalizedEmail,
            TimeSpan.FromMinutes(5), ct);

        return resetToken;
    }
}