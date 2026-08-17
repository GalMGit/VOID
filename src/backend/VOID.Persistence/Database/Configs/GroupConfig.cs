using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VOID.Domain.Models.Groups;

namespace VOID.Persistence.Database.Configs;

public class GroupConfig : IEntityTypeConfiguration<GroupChat>
{
    public void Configure(EntityTypeBuilder<GroupChat> builder)
    {
        builder.ToTable("GroupChats");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.ChatName)
            .IsRequired()
            .HasMaxLength(15);

        builder.Property(g => g.Description)
            .HasMaxLength(100);

        builder.Property(g => g.ImageUrl)
            .HasMaxLength(500);

        builder.HasOne(g => g.Owner)
            .WithMany(u => u.OwnedGroupChats)
            .HasForeignKey(g => g.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(g => g.GroupMembers)
            .WithOne(gm => gm.Group)
            .HasForeignKey(gm => gm.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(g => g.Messages)
            .WithOne(m => m.GroupChat)
            .HasForeignKey(m => m.GroupChatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(g => g.ChatName);
    }
}