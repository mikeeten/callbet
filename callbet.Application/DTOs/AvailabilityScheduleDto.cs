using System;

namespace callbet.Application.DTOs
{
    public class AvailabilityScheduleDto
    {
        public Guid Id { get; set; }
        public Guid ProfessionalProfileId { get; set; }
        public int DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}