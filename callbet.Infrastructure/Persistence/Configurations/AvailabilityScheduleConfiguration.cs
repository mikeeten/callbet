using callbet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace callbet.Infrastructure.Persistence.Configurations
{
    public class AvailabilityScheduleConfiguration : IEntityTypeConfiguration<AvailabilitySchedule>
    {
        public void Configure(EntityTypeBuilder<AvailabilitySchedule> builder)
        {
            builder.HasKey(a => a.Id);
            builder.HasIndex(a => new { a.ProfessionalProfileId, a.DayOfWeek, a.StartTime, a.EndTime }).IsUnique();
        }
    }
}