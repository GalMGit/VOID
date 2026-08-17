using System;

namespace VOID.Application.UseCases.Groups.Queries.SearchUsersForGroup;

public sealed record SearchUsersForGroupQuery(
    string SearchTerm, 
    Guid UserId, 
    Guid GroupId);