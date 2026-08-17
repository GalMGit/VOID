using System;
using System.Collections.Generic;
using VOID.Domain.Enums.Roles.App;
using VOID.Domain.Models.Base;
using VOID.Domain.Models.Channels;
using VOID.Domain.Models.Chats;
using VOID.Domain.Models.Groups;
using VOID.Domain.Models.Messages;

namespace VOID.Domain.Models.Users;

public class User : BaseModel
{
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public string? AboutMe { get; set; }
    public string? AvatarUrl { get; set; }
    public AppRole AppRole { get; set; }
    public DateTime LastSeen { get; set; }
    public bool IsOnline { get; set; }
    public ICollection<Message> Messages { get; set; } = [];
    public ICollection<GroupChat> OwnedGroupChats { get; set; } = [];
    public ICollection<ChannelChat> OwnedChannels { get; set; } = [];
    public ICollection<ChatInterlocutor> ChatInterlocutors { get; set; } = [];
    public ICollection<GroupMember> GroupMemberships { get; set; } = [];
    public ICollection<ChannelFollower> ChannelFollowings { get; set; } = [];
    public ICollection<Like> Likes { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}