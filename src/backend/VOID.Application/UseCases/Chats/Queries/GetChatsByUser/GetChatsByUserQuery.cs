using System;
using VOID.Shared.Contracts.DTOs.Paginations;

namespace VOID.Application.UseCases.Chats.Queries.GetChatsByUser;

public sealed record GetChatsByUserQuery(
    Guid UserId,
    PaginationRequest Pagination);