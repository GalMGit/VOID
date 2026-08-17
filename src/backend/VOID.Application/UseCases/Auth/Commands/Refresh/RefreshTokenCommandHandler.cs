using System;
using System.Threading;
using System.Threading.Tasks;
using VOID.Application.Abstractions.IRepositories.IUserRepositories;
using VOID.Application.Abstractions.IServices.IAuthServices;
using VOID.Application.Exceptions;
using VOID.Domain.Models.Users;
using VOID.Shared.Contracts.DTOs.Auth.Login;

namespace VOID.Application.UseCases.Auth.Commands.Refresh;

public sealed class RefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IJwtProvider jwtProvider) 
{
    public async Task<LoginDto> Handle(
        RefreshTokenCommand request, 
        CancellationToken ct)
    {
        var storedToken = await refreshTokenRepository.GetByTokenAsync(
                              request.Dto.RefreshToken, ct) 
                          ?? throw new ForbiddenException();

        if (storedToken.Revoked)
            throw new ForbiddenException();

        if (storedToken.ExpiresAt < DateTime.UtcNow)
            throw new ForbiddenException();

        var user = storedToken.User;

        var newAccessToken = jwtProvider.GenerateToken(user);

        var newRefreshToken = jwtProvider.GenerateRefreshToken();

        storedToken.Revoked = true;

        await refreshTokenRepository.UpdateAsync(
            storedToken, ct);

        await refreshTokenRepository.CreateAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        }, ct);

        return new LoginDto
        {
            Token = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }
}