using System;

namespace VOID.Application.UseCases.Groups.Events.MemberDeleted;

public sealed record MemberDeletedEvent(
    Guid GroupId, 
    Guid MemberId,
    Guid UserId);