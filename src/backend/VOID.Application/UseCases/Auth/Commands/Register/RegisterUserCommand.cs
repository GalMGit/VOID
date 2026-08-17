using VOID.Shared.Contracts.DTOs.Auth.Register;

namespace VOID.Application.UseCases.Auth.Commands.Register;

public sealed record RegisterUserCommand(
    RegisterUserDto Dto);