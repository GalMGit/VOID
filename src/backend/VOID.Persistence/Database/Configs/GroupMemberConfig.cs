using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VOID.Domain.Enums.Roles.Group;
using VOID.Domain.Models.Groups;

namespace VOID.Persistence.Database.Configs;

public class GroupMemberConfig : IEntityTypeConfiguration<GroupMember>
{
    public void Configure(EntityTypeBuilder<GroupMember> builder)
    {
        builder.ToTable("GroupMembers");

        builder.HasKey(gm => gm.Id);

        builder.Property(gm => gm.GroupRole)
            .HasConversion<int>()
            .HasDefaultValue(GroupRole.Member);

        builder.Property(gm => gm.IsBanned)
            .HasDefaultValue(false);

        builder.HasIndex(gm => new { gm.MemberId, gm.GroupId })
            .IsUnique();

        builder.HasOne(gm => gm.Member)
            .WithMany()
            .HasForeignKey(gm => gm.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(gm => gm.Group)
            .WithMany(g => g.GroupMembers)
            .HasForeignKey(gm => gm.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}