using System;

namespace VOID.Application.UseCases.Chats.Queries.GetById;

public sealed record GetChatByIdQuery(
    Guid ChatId, 
    Guid UserId);