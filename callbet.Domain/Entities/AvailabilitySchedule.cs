using System;

namespace callbet.Domain.Entities
{
    public class AvailabilitySchedule
    {
        public Guid Id { get; set; }
        public Guid ProfessionalProfileId { get; set; }
        public int DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public ProfessionalProfile ProfessionalProfile { get; set; } = null!;
    }
}