namespace VOID.Shared.Contracts.DTOs.Chats;

public class FullChatDto
{
    public Guid Id { get; set; }
    public string ChatName { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime InterlocutorLastSeen { get; set; }
    public Guid InterlocutorId { get; set; }
    public bool InterlocutorOnline { get; set; }
    public string? InterlocutorAboutMe { get; set; }
    public string InterlocutorUsername { get; set; }
    public int MessageCount { get; set; }
}
