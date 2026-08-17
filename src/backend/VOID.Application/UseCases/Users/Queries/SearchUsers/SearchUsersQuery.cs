using System;

namespace VOID.Application.UseCases.Users.Queries.SearchUsers;

public sealed record SearchUsersQuery(
    string Username, 
    Guid UserId);