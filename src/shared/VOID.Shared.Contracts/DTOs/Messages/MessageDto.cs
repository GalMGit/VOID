using VOID.Shared.Contracts.Enums.Chats;
using VOID.Shared.Contracts.Enums.Messages;

namespace VOID.Shared.Contracts.DTOs.Messages;

public class MessageDto
{
    public Guid Id { get; set; }
    public string AuthorName { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? AvatarUrl { get; set; }
    public Guid SenderId { get; set; }
    public bool IsMine { get; set; }
    public string? MediaUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool IsEdited { get; set; }
    public string? Text { get; set; }
    public bool IsRead { get; set; }
    public DateTime ReadAt { get; set; }
    public ChatType ChatType { get; set; }
    public MessageType MessageType { get; set; }
    public Guid ParentId { get; set; }
}