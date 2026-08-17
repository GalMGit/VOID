using System;
using VOID.Domain.Models.Base;
using VOID.Domain.Models.Users;

namespace VOID.Domain.Models.Channels;

public class Like : BaseModel
{
    public Guid UserId { get; set; }
    public User User { get; set; }
    public Post Post { get; set; }
    public Guid PostId { get; set; }
}
