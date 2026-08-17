using System;
using VOID.Shared.Contracts.DTOs.Groups;

namespace VOID.Application.UseCases.Groups.Events.Created;

public sealed record GroupCreatedEvent(
    GroupDto Group, 
    Guid UserId);