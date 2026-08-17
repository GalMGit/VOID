using System;
using VOID.Shared.Contracts.DTOs.Groups;

namespace VOID.Application.UseCases.Groups.Commands.Create;

public sealed record CreateGroupCommand(
    CreateGroupDto Dto, 
    Guid UserId);