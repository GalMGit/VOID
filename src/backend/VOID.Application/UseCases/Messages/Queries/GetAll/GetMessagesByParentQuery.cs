using System;
using VOID.Shared.Contracts.DTOs.Paginations;
using VOID.Shared.Contracts.Enums.Chats;

namespace VOID.Application.UseCases.Messages.Queries.GetAll;

public sealed record GetMessagesByParentQuery(
    Guid ParentId, 
    Guid UserId, 
    ChatType ChatType,
    PaginationRequest Pagination);