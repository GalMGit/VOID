using System;
using System.Collections.Generic;
using VOID.Domain.Models.Base;
using VOID.Domain.Models.Users;

namespace VOID.Domain.Models.Channels;

public class Post : BaseModel
{
    public User Author { get; set; }
    public Guid AuthorId { get; set; }
    public string? Text { get; set; }
    public string? ImageUrl { get; set; }
    public ICollection<Like> Likes { get; set; } = [];
    public ChannelChat Channel { get; set; }
    public Guid ChannelId { get; set; }
}
