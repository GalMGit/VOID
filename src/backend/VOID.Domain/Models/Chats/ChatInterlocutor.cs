using System;
using VOID.Domain.Models.Base;
using VOID.Domain.Models.Users;

namespace VOID.Domain.Models.Chats;

public class ChatInterlocutor : BaseModel
{
    public User User { get; set; }
    public Guid UserId { get; set; }
    public Guid ChatId { get; set; }
    public Chat Chat { get; set; }
}
