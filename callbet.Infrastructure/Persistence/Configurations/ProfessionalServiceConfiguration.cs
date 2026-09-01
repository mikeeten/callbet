using callbet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace callbet.Infrastructure.Persistence.Configurations
{
    public class ProfessionalServiceConfiguration : IEntityTypeConfiguration<ProfessionalService>
    {
        public void Configure(EntityTypeBuilder<ProfessionalService> builder)
        {
            builder.HasKey(ps => ps.Id);
            builder.HasIndex(ps => new { ps.ProfessionalProfileId, ps.ServiceId }).IsUnique();

            builder.Property(ps => ps.CustomPrice).HasColumnType("decimal(18, 2)");

            builder.HasOne(ps => ps.Service).WithMany(s => s.ProfessionalServices).HasForeignKey(ps => ps.ServiceId);
            builder.HasOne(ps => ps.ProfessionalProfile).WithMany(p => p.Services).HasForeignKey(ps => ps.ProfessionalProfileId);
        }
    }
}