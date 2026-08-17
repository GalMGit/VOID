namespace VOID.Shared.Contracts.DTOs.Chats;

public class ChatInterlocutorDto
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public Guid UserId { get; set; }
    public DateTime LastSeen { get; set; }
}
