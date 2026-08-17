using VOID.Shared.Contracts.DTOs.Auth.Register;

namespace VOID.Application.UseCases.Auth.Commands.ConfirmEmail;

public record ConfirmEmailCommand(
    ConfirmEmailDto Dto);