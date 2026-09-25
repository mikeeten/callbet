using System;
using System.Collections.Generic;

namespace callbet.Application.DTOs
{
    public class ProfessionalProfileDetailsDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public string? Headline { get; set; }
        public string? Bio { get; set; }
        public int ServiceRadiusKm { get; set; }
        public int YearsOfExperience { get; set; }
        public decimal OverallRating { get; set; }
        public int CompletedJobsCount { get; set; }
        public bool IsVerified { get; set; }

        public AddressDto? BaseAddress { get; set; }
        public IEnumerable<AddressDto> Addresses { get; set; } = new List<AddressDto>();
        public ResumeDto? Resume { get; set; }
        public IEnumerable<CertificateDto> Certificates { get; set; } = new List<CertificateDto>();
        public IEnumerable<PortfolioItemDto> PortfolioItems { get; set; } = new List<PortfolioItemDto>();
        public IEnumerable<AvailabilityScheduleDto> AvailabilitySchedules { get; set; } = new List<AvailabilityScheduleDto>();
        public IEnumerable<ReviewDto> Reviews { get; set; } = new List<ReviewDto>();
    }
}
