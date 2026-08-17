using System;

namespace VOID.Application.Abstractions.IServices.IMediaServices;

public interface IMediaUrlService
{
    string? GetAvatarUrl(string? relativePath);
    string GetMessageMediaUrl(Guid messageId);
    string GetMessageThumbnailUrl(Guid messageId);
}