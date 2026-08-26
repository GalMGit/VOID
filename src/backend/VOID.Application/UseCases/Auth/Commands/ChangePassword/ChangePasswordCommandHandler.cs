using System.Threading;
using System.Threading.Tasks;
using VOID.Application.Abstractions.IRepositories.IUserRepositories;
using VOID.Application.Abstractions.IServices.IAuthServices;
using VOID.Application.Exceptions;

namespace VOID.Application.UseCases.Auth.Commands.ChangePassword;

public sealed class ChangePasswordCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher)
{
    public async Task Handle(
        ChangePasswordCommand request,
        CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(
            request.UserId, ct)
                   ?? throw new NotFoundException("Пользователь не найден");

        if (!passwordHasher.VerifyHash(
                request.Dto.OldPassword,
                user.PasswordHash))
            throw new ForbiddenException("Пароль не верный");

        var newHashedPassword = passwordHasher.GenerateHash(
            request.Dto.NewPassword);

        await userRepository.ChangePasswordAsync(
            request.UserId,
            newHashedPassword, ct);
    }
}