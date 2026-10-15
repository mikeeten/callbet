using callbet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace callbet.Infrastructure.Persistence.Configurations
{
    public class ReportConfiguration : IEntityTypeConfiguration<Report>
    {
        public void Configure(EntityTypeBuilder<Report> builder)
        {
            builder.HasKey(r => r.Id);
            builder.Property(r => r.ReporterUserId).IsRequired();
            builder.Property(r => r.ReportedUserId).IsRequired(false);
            builder.Property(r => r.Reason).IsRequired().HasMaxLength(500);
            builder.Property(r => r.Details).HasMaxLength(2000);
        }
    }
}
