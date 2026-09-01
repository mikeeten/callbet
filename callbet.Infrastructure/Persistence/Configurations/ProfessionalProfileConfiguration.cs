using callbet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace callbet.Infrastructure.Persistence.Configurations
{
    public class ProfessionalProfileConfiguration : IEntityTypeConfiguration<ProfessionalProfile>
    {
        public void Configure(EntityTypeBuilder<ProfessionalProfile> builder)
        {
            builder.HasKey(p => p.Id);
            builder.HasIndex(p => p.UserId).IsUnique();

            builder.Property(p => p.Headline).HasMaxLength(255);
            builder.Property(p => p.OverallRating).HasColumnType("decimal(3, 2)");

            builder.HasOne(p => p.User)
                .WithOne(u => u.ProfessionalProfile)
                .HasForeignKey<ProfessionalProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Certificates)
                .WithOne(c => c.ProfessionalProfile)
                .HasForeignKey(c => c.ProfessionalProfileId);

            builder.HasMany(p => p.AvailabilitySchedules)
                .WithOne(a => a.ProfessionalProfile)
                .HasForeignKey(a => a.ProfessionalProfileId);
        }
    }
}