using System;
using VOID.Domain.Enums.Types.Chat;
using VOID.Domain.Enums.Types.Message;
using VOID.Domain.Models.Base;
using VOID.Domain.Models.Chats;
using VOID.Domain.Models.Groups;
using VOID.Domain.Models.Users;

namespace VOID.Domain.Models.Messages;

public class Message : BaseModel
{
    public string? Text { get; set; }
    public MessageType MessageType { get; set; }
    public string? MediaUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public ChatType ChatType { get; set; }
    public Guid SenderId { get; set; }
    public string? ContentType { get; set; }
    public User Sender { get; set; }

    public bool IsRead { get; set; }
    public bool IsEdited { get; set; }
    
    public DateTime ReadAt { get; set; }

    public Guid? ChatId { get; set; }
    public Chat? Chat { get; set; }

    public Guid? GroupChatId { get; set; }
    public GroupChat? GroupChat { get; set; }
}