using System;
using VOID.Application.Abstractions.IServices.IMediaServices;

namespace VOID.Application.UseCases.Images.Commands.UpdateGroupImage;

public sealed record UpdateGroupImageCommand(
    Guid UserId,
    Guid GroupId,
    UploadFile? Media);