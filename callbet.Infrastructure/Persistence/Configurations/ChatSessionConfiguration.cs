using callbet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace callbet.Infrastructure.Persistence.Configurations
{
    public class ChatSessionConfiguration : IEntityTypeConfiguration<ChatSession>
    {
        public void Configure(EntityTypeBuilder<ChatSession> builder)
        {
            builder.HasKey(cs => cs.Id);

            builder.HasIndex(cs => new { cs.CustomerId, cs.ProfessionalId }).IsUnique();

            builder.HasMany(cs => cs.Messages)
                .WithOne(cm => cm.ChatSession)
                .HasForeignKey(cm => cm.ChatSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}