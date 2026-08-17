using VOID.Shared.Contracts.DTOs.Auth.Login;

namespace VOID.Application.UseCases.Auth.Commands.Login;

public sealed record LoginUserCommand(
    LoginUserDto Dto);
