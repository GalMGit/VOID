using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VOID.Domain.Enums.Roles.App;
using VOID.Domain.Models.Users;

namespace VOID.Persistence.Database.Configs;

public class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(15);

        builder.HasIndex(u => u.Username)
            .IsUnique();

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(300);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(15);

        builder.Property(u => u.AboutMe)
            .HasMaxLength(50);

        builder.Property(u => u.AppRole)
            .HasConversion<string>()
            .HasDefaultValue(AppRole.User);

        builder.HasMany(u => u.OwnedGroupChats)
            .WithOne(g => g.Owner)
            .HasForeignKey(g => g.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.OwnedChannels)
            .WithOne(c => c.Owner)
            .HasForeignKey(c => c.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(u => u.CreatedAt);
    }
}