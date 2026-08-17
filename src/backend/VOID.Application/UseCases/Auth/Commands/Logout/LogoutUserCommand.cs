using VOID.Shared.Contracts.DTOs.Auth.Logout;

namespace VOID.Application.UseCases.Auth.Commands.Logout;

public sealed record LogoutUserCommand(
    LogoutDto Dto);