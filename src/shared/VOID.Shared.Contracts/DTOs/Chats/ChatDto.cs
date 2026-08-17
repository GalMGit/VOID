namespace VOID.Shared.Contracts.DTOs.Chats;

public class ChatDto
{
    public Guid Id { get; set; }
    public string ChatName { get; set; }
    public string? ImageUrl { get; set; }
    public string? LastMessage { get; set; }
    public DateTime? LastMessageDate { get; set; }
    public bool InterlocutorOnline { get; set; }
    public Guid InterlocutorId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UnreadCount { get; set; }
}

