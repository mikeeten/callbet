using System;

namespace callbet.Domain.Entities
{
    public class ProfessionalService
    {
        public Guid Id { get; set; }
        public Guid ProfessionalProfileId { get; set; }
        public Guid ServiceId { get; set; }
        public decimal? CustomPrice { get; set; }
        public int? ExperienceYears { get; set; }

        public ProfessionalProfile ProfessionalProfile { get; set; } = null!;
        public Service Service { get; set; } = null!;
    }
}