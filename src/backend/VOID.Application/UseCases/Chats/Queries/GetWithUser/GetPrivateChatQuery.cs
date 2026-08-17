using System;

namespace VOID.Application.UseCases.Chats.Queries.GetWithUser;

public sealed record GetPrivateChatQuery(
    Guid CurrentUserId,
    Guid UserId);