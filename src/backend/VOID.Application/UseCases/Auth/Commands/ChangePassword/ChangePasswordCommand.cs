using System;
using VOID.Shared.Contracts.DTOs.Auth.ChangePassword;

namespace VOID.Application.UseCases.Auth.Commands.ChangePassword;

public sealed record ChangePasswordCommand(
    ChangePasswordDto Dto,
    Guid UserId);