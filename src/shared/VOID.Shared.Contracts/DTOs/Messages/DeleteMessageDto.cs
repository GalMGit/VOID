using VOID.Shared.Contracts.Enums.Messages;

namespace VOID.Shared.Contracts.DTOs.Messages;

public class DeleteMessageDto
{
    public string? ImageUrl { get; set; }
    public MessageType MessageType { get; set; }
    public string? ThumbnailUrl { get; set; }
}