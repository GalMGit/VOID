using VOID.Shared.Contracts.DTOs.Auth.ResetPassword;

namespace VOID.Application.UseCases.Auth.Commands.ResetPassword;

public sealed record StartResetPasswordCommand(
    StartResetPasswordDto Dto);
