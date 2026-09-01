using System;
using System.Collections.Generic;

namespace callbet.Domain.Entities
{
    public class ProfessionalProfile
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? Headline { get; set; }
        public string? Bio { get; set; }
        public int ServiceRadiusKm { get; set; } = 15;
        public int YearsOfExperience { get; set; } = 0;
        public decimal OverallRating { get; set; } = 0.00m;
        public int CompletedJobsCount { get; set; } = 0;
        public bool IsVerified { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
        public Resume? Resume { get; set; }
        public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
        public ICollection<ProfessionalService> Services { get; set; } = new List<ProfessionalService>();
        public ICollection<AvailabilitySchedule> AvailabilitySchedules { get; set; } = new List<AvailabilitySchedule>();
    }
}