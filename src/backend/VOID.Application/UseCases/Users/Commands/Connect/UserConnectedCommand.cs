using System;

namespace VOID.Application.UseCases.Users.Commands.Connect;

public sealed record UserConnectedCommand(
    Guid UserId);