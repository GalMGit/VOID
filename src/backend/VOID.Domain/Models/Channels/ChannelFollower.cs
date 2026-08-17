using System;
using VOID.Domain.Models.Base;
using VOID.Domain.Models.Users;

namespace VOID.Domain.Models.Channels;

public class ChannelFollower : BaseModel
{
    public Guid ChannelId { get; set; }
    public ChannelChat Channel { get; set; }
    public bool IsBanned { get; set; }
    public Guid FollowerId { get; set; }
    public User Follower { get; set; }
}
