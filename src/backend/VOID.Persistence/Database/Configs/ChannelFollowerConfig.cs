using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VOID.Domain.Models.Channels;

namespace VOID.Persistence.Database.Configs;

public class ChannelFollowerConfig : IEntityTypeConfiguration<ChannelFollower>
{
    public void Configure(EntityTypeBuilder<ChannelFollower> builder)
    {
        builder.ToTable("ChannelFollowers");

        builder.HasKey(cf => cf.Id);

        builder.Property(cf => cf.IsBanned)
            .HasDefaultValue(false);

        builder.HasIndex(cf => new { cf.FollowerId, cf.ChannelId })
            .IsUnique();

        builder.HasOne(cf => cf.Follower)
            .WithMany()
            .HasForeignKey(cf => cf.FollowerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cf => cf.Channel)
            .WithMany(c => c.Followers)
            .HasForeignKey(cf => cf.ChannelId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}