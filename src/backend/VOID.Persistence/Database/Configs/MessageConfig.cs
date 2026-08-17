using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VOID.Domain.Enums.Types.Message;
using VOID.Domain.Models.Messages;

namespace VOID.Persistence.Database.Configs;

public class MessageConfig : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Text)
            .HasMaxLength(4000);

        builder.Property(m => m.MediaUrl)
            .HasMaxLength(500);

        builder.Property(m => m.ThumbnailUrl)
            .HasMaxLength(500);

        builder.Property(m => m.MessageType)
            .HasConversion<string>()
            .HasDefaultValue(MessageType.Text);

        builder.Property(m => m.ChatType)
           .HasConversion<string>();

        builder.Property(m => m.IsRead)
            .HasDefaultValue(false);

        builder.Property(m => m.IsEdited)
            .HasDefaultValue(false);

        builder.HasOne(m => m.Sender)
            .WithMany(u => u.Messages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Chat)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.GroupChat)
            .WithMany(g => g.Messages)
            .HasForeignKey(m => m.GroupChatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.SenderId);
        builder.HasIndex(m => m.CreatedAt);
        builder.HasIndex(m => new { m.ChatId, m.CreatedAt });
        builder.HasIndex(m => new { m.GroupChatId, m.CreatedAt });
    }
}