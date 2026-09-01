using System;

namespace callbet.Application.DTOs
{
    public class ProfessionalServiceDto
    {
        public Guid Id { get; set; }
        public Guid ProfessionalProfileId { get; set; }
        public Guid ServiceId { get; set; }
        public decimal? CustomPrice { get; set; }
        public int? ExperienceYears { get; set; }
    }
}