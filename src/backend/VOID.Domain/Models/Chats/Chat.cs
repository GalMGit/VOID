using System;
using System.Collections.Generic;
using VOID.Domain.Models.Base;
using VOID.Domain.Models.Messages;
using VOID.Domain.Models.Users;

namespace VOID.Domain.Models.Chats;

public class Chat : BaseModel
{
    public User Creator { get; set; }
    public Guid CreatorId { get; set; }
    public ICollection<ChatInterlocutor> Interlocutors { get; set; } = [];
    public DateTime? LastMessageDate { get; set; }
    public string? LastMessage { get; set; }
    public ICollection<Message> Messages { get; set; } = [];
}
