using System;

namespace VOID.Application.UseCases.Groups.Commands.LeaveFromGroup;

public sealed record LeaveFromGroupCommand(
    Guid GroupId, 
    Guid UserId);