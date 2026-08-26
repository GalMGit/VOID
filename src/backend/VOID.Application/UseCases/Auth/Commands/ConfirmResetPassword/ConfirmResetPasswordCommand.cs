using VOID.Shared.Contracts.DTOs.Auth.ConfirmResetPassword;

namespace VOID.Application.UseCases.Auth.Commands.ConfirmResetPassword;

public sealed record ConfirmResetPasswordCommand(
    ConfirmResetPasswordDto Dto);