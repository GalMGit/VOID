using System;
using VOID.Application.Abstractions.IServices.IMediaServices;

namespace VOID.Application.UseCases.Images.Commands.UpdateAvatar;

public sealed record UpdateAvatarCommand(
    Guid UserId,
    UploadFile? Media);