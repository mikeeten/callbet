using callbet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace callbet.Infrastructure.Persistence.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.HasKey(r => r.Id);

            builder.HasIndex(r => r.JobId).IsUnique();
            builder.HasIndex(r => r.RevieweeId);

            builder.HasOne(r => r.Reply)
                .WithOne()
                .HasForeignKey<ReviewReply>(rr => rr.ReviewId);
        }
    }
}