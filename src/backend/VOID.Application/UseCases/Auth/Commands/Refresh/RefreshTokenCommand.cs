using VOID.Shared.Contracts.DTOs.Auth.Token;

namespace VOID.Application.UseCases.Auth.Commands.Refresh;

public sealed record RefreshTokenCommand(
    RefreshTokenDto Dto);