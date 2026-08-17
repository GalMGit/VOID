using System;
using System.Collections.Generic;
using VOID.Shared.Contracts.DTOs.Groups;

namespace VOID.Application.UseCases.Groups.Events.MembersAdded;

public sealed record MembersAddedEvent(
    GroupDto Group, 
    List<Guid> MembersIds,
    Guid SenderId);