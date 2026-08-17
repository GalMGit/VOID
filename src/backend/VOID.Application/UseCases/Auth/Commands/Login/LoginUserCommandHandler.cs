using System;
using System.Threading;
using System.Threading.Tasks;
using VOID.Application.Abstractions.IRepositories.IUserRepositories;
using VOID.Application.Abstractions.IServices.IAuthServices;
using VOID.Application.Exceptions;
using VOID.Domain.Models.Users;
using VOID.Shared.Contracts.DTOs.Auth.Login;

namespace VOID.Application.UseCases.Auth.Commands.Login;

public sealed class LoginUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtProvider jwtProvider,
    IRefreshTokenRepository refreshTokenRepository)
{
    public async Task<LoginDto> Handle(
        LoginUserCommand request, 
        CancellationToken ct)
    {
        var user = await userRepository.GetByUsernameAsync(
            request.Dto.Username, ct);

        if (user is null || user.IsDeleted || !passwordHasher.VerifyHash(
                request.Dto.Password, 
                user.PasswordHash))
            throw new NotFoundException("Неверный логин или пароль");

        var token = jwtProvider.GenerateToken(user);

        var refreshToken = jwtProvider.GenerateRefreshToken();

        var tokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };

        await refreshTokenRepository.CreateAsync(
            tokenEntity, ct);

        return new LoginDto
        {
            Token = token,
            RefreshToken = refreshToken
        };
    }
}