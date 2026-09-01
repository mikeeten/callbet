using callbet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace callbet.Infrastructure.Persistence.Configurations
{
    public class JobConfiguration : IEntityTypeConfiguration<Job>
    {
        public void Configure(EntityTypeBuilder<Job> builder)
        {
            builder.HasKey(j => j.Id);

            builder.Property(j => j.Description).IsRequired();
            builder.Property(j => j.Price).HasColumnType("decimal(18, 2)");

            builder.Property(j => j.Status).HasConversion<string>().HasMaxLength(50);
        }
    }
}