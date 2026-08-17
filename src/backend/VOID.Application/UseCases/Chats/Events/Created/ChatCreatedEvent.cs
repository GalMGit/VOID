using System;
using VOID.Shared.Contracts.DTOs.Chats;

namespace VOID.Application.UseCases.Chats.Events.Created;

public sealed record ChatCreatedEvent(
    Guid ChatId, 
    Guid CreatorId, 
    Guid RecipientId, 
    ChatDto CreatorChat,
    ChatDto RecipientChat);