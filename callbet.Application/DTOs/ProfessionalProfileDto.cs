using System;
using System.Collections.Generic;

namespace callbet.Application.DTOs
{
    public class ProfessionalProfileDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? Headline { get; set; }
        public string? Bio { get; set; }
        public double ServiceRadiusKm { get; set; }
        public int YearsOfExperience { get; set; }
        public decimal OverallRating { get; set; }
        public int CompletedJobsCount { get; set; }
        public bool IsVerified { get; set; }

        public ResumeDto? Resume { get; set; }
        public ICollection<CertificateDto> Certificates { get; set; } = new List<CertificateDto>();
        public ICollection<ProfessionalServiceDto> Services { get; set; } = new List<ProfessionalServiceDto>();
        public ICollection<AvailabilityScheduleDto> AvailabilitySchedules { get; set; } = new List<AvailabilityScheduleDto>();
    }
}