using System;
using VOID.Shared.Contracts.DTOs.Groups;

namespace VOID.Application.UseCases.Groups.Commands.AddMembers;

public sealed record AddMembersToGroupCommand(
    AddGroupMembersDto Dto, 
    Guid GroupId, 
    Guid UserId);