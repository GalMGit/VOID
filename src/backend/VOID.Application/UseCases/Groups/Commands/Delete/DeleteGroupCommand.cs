using System;

namespace VOID.Application.UseCases.Groups.Commands.Delete;

public sealed record DeleteGroupCommand(
    Guid GroupId, 
    Guid UserId);