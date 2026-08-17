using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VOID.Domain.Models.Chats;

namespace VOID.Persistence.Database.Configs;

public class ChatInterlocutorConfig : IEntityTypeConfiguration<ChatInterlocutor>
{
    public void Configure(EntityTypeBuilder<ChatInterlocutor> builder)
    {
        builder.ToTable("ChatParticipants");

        builder.HasKey(cp => cp.Id);

        builder.HasIndex(cp => new { cp.UserId, cp.ChatId })
            .IsUnique();

        builder.HasOne(cp => cp.User)
            .WithMany(u => u.ChatInterlocutors)
            .HasForeignKey(cp => cp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cp => cp.Chat)
            .WithMany(c => c.Interlocutors)
            .HasForeignKey(cp => cp.ChatId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}