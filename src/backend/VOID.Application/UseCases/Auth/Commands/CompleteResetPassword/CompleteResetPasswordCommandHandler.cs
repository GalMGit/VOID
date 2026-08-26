using System.Threading;
using System.Threading.Tasks;
using VOID.Application.Abstractions.IRepositories.IUserRepositories;
using VOID.Application.Abstractions.IServices.IAuthServices;
using VOID.Application.Abstractions.IServices.ICacheServices;
using VOID.Application.Exceptions;

namespace VOID.Application.UseCases.Auth.Commands.CompleteResetPassword;

public sealed class CompleteResetPasswordCommandHandler(
    IUserRepository userRepository,
    ICacheService cacheService,
    IPasswordHasher passwordHasher)
{
    public async Task Handle(
        CompleteResetPasswordCommand request,
        CancellationToken ct)
    {
        var tokenKey = $"reset-password:confirmed:{request.Dto.ResetToken}";

        var email = await cacheService.GetAsync<string>(
            tokenKey,
            ct);
        
        if (email is null)
            throw new NotFoundException(
                "Ссылка для сброса пароля недействительна или истекла");
        
        var user = await userRepository.GetByEmailAsync(
            email,
            ct);
        
        if (user is null)
            throw new NotFoundException(
                "Пользователь не найден");

        var hashedPassword = passwordHasher.GenerateHash(
            request.Dto.NewPassword);

        await userRepository.ChangePasswordAsync(
            user.Id, 
            hashedPassword, ct);

        await cacheService.RemoveAsync(
            tokenKey, ct);

    }
}