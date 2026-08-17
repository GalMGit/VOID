using System;
using System.Collections.Generic;
using VOID.Domain.Models.Base;
using VOID.Domain.Models.Users;

namespace VOID.Domain.Models.Channels;

public class ChannelChat : BaseModel
{
    public string ChannelName { get; set; }
    public string? Description { get; set; }
    public Guid OwnerId { get; set; }
    public User Owner { get; set; }
    public ICollection<ChannelFollower> Followers { get; set; } = [];
    public string? ImageUrl { get; set; }
    public DateTime? LastPostDate { get; set; }
    public ICollection<Post> Posts { get; set; } = [];
}
