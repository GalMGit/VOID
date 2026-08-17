using System;
using System.Collections.Generic;
using VOID.Domain.Models.Base;
using VOID.Domain.Models.Messages;
using VOID.Domain.Models.Users;

namespace VOID.Domain.Models.Groups;

public class GroupChat : BaseModel
{
    public string ChatName { get; set; }
    public string? Description { get; set; }
    public Guid OwnerId { get; set; }
    public User Owner { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime? LastMessageDate { get; set; }
    public ICollection<GroupMember> GroupMembers { get; set; } = [];
    public ICollection<Message> Messages { get; set; } = [];
}
