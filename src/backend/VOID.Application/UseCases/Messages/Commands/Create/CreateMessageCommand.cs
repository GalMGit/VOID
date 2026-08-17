using System;
using VOID.Application.Abstractions.IServices.IMediaServices;
using VOID.Shared.Contracts.DTOs.Messages;

namespace VOID.Application.UseCases.Messages.Commands.Create;

public sealed record CreateMessageCommand(
    CreateMessageDto Dto,
    Guid UserId, 
    UploadFile? Media);