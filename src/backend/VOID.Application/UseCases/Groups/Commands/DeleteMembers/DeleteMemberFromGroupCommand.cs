using System;

namespace VOID.Application.UseCases.Groups.Commands.DeleteMembers;

public sealed record DeleteMemberFromGroupCommand(
    Guid GroupId,
    Guid MemberId,
    Guid UserId);