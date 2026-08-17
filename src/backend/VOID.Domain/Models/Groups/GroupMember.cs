using System;
using VOID.Domain.Enums.Roles.Group;
using VOID.Domain.Models.Base;
using VOID.Domain.Models.Users;

namespace VOID.Domain.Models.Groups;

public class GroupMember : BaseModel
{
    public Guid MemberId { get; set; }
    public GroupRole GroupRole { get; set; }
    public User Member { get; set; }
    public bool IsBanned { get; set; }
    public Guid GroupId { get; set; }
    public GroupChat Group { get; set; }
}
