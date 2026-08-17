using System;
using VOID.Shared.Contracts.DTOs.Users.Accounts;

namespace VOID.Application.UseCases.Users.Commands.Update;

public sealed record UpdateUserCommand(
    UpdateUserDto Dto,
    Guid UserId);