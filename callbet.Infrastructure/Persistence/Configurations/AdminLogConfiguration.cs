using callbet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace callbet.Infrastructure.Persistence.Configurations
{
    public class AdminLogConfiguration : IEntityTypeConfiguration<AdminLog>
    {
        public void Configure(EntityTypeBuilder<AdminLog> builder)
        {
            builder.HasKey(al => al.Id);

            builder.Property(al => al.Action).IsRequired().HasMaxLength(255);
            builder.Property(al => al.TargetEntity).HasMaxLength(100);
        }
    }
}