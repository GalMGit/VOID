using System;

namespace VOID.Application.UseCases.Chats.Queries.GetRelatedIds;

public sealed record GetRelatedIdsQuery(
    Guid UserId);