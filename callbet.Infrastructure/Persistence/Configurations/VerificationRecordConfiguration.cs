using callbet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace callbet.Infrastructure.Persistence.Configurations
{
    public class VerificationRecordConfiguration : IEntityTypeConfiguration<VerificationRecord>
    {
        public void Configure(EntityTypeBuilder<VerificationRecord> builder)
        {
            builder.HasKey(vr => vr.Id);
            builder.Property(vr => vr.DocumentType).IsRequired().HasMaxLength(100);
            builder.Property(vr => vr.DocumentUrl).IsRequired();
            builder.HasIndex(vr => vr.UserId);
        }
    }
}