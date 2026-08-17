using System;
using Microsoft.EntityFrameworkCore;
using VOID.Domain.Models.Channels;
using VOID.Domain.Models.Chats;
using VOID.Domain.Models.Groups;
using VOID.Domain.Models.Messages;
using VOID.Domain.Models.Users;
using VOID.Persistence.Database.Configs;

namespace VOID.Persistence.Database.Context;

public class VoidDbContext : DbContext
{
    public VoidDbContext(DbContextOptions<VoidDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<GroupChat> Groups => Set<GroupChat>();
    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
    public DbSet<ChannelChat> Channels => Set<ChannelChat>();
    public DbSet<ChannelFollower> Subscribers => Set<ChannelFollower>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Like> Likes => Set<Like>();
    public DbSet<ChatInterlocutor> ChatInterlocutors => Set<ChatInterlocutor>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UserConfig());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfig());
        modelBuilder.ApplyConfiguration(new ChatConfig());
        modelBuilder.ApplyConfiguration(new GroupConfig());
        modelBuilder.ApplyConfiguration(new ChannelConfig());
        modelBuilder.ApplyConfiguration(new GroupMemberConfig());
        modelBuilder.ApplyConfiguration(new ChannelFollowerConfig());
        modelBuilder.ApplyConfiguration(new MessageConfig());
        modelBuilder.ApplyConfiguration(new PostConfig());
        modelBuilder.ApplyConfiguration(new LikeConfig());
        modelBuilder.ApplyConfiguration(new ChatInterlocutorConfig());
    }
}
