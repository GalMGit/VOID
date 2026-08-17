using System;
using VOID.Shared.Contracts.DTOs.Paginations;

namespace VOID.Application.UseCases.Groups.Queries.GetGroupsByUser;

public sealed record GetGroupsByUserQuery(
    Guid UserId,
    PaginationRequest Pagination);