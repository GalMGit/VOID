using System;

namespace VOID.Application.UseCases.Groups.Queries.GetById;

public sealed record GetGroupByIdQuery(
    Guid UserId, 
    Guid GroupId);