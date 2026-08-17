using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VOID.Domain.Models.Channels;

namespace VOID.Persistence.Database.Configs;

public class ChannelConfig : IEntityTypeConfiguration<ChannelChat>
{
    public void Configure(EntityTypeBuilder<ChannelChat> builder)
    {
        builder.ToTable("ChannelChats");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.ChannelName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.ImageUrl)
            .HasMaxLength(500);


        builder.HasOne(c => c.Owner)
            .WithMany(u => u.OwnedChannels)
            .HasForeignKey(c => c.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Followers)
            .WithOne(cf => cf.Channel)
            .HasForeignKey(cf => cf.ChannelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Posts)
            .WithOne(p => p.Channel)
            .HasForeignKey(p => p.ChannelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.ChannelName);
    }
}