using VOID.Shared.Contracts.DTOs.Auth.ResetPassword;

namespace VOID.Application.UseCases.Auth.Commands.CompleteResetPassword;

public sealed record CompleteResetPasswordCommand(
    CompleteResetPasswordDto Dto);