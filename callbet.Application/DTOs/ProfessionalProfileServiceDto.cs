using System;
using callbet.Domain.Entities;

namespace callbet.Application.DTOs;

public class ProfessionalProfileServiceDto
{
    public Guid Id { get; set; } // ProfessionalService Id
    public Guid ProfessionalProfileId { get; set; }
    public Guid UserId { get; set; }
    public string ProfessionalName { get; set; } = null!;
    public string? ProfessionalHeadline { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public decimal OverallRating { get; set; }
    public int CompletedJobsCount { get; set; }
    public bool IsVerified { get; set; }
    public int? ProfessionalExperienceYears { get; set; }

    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = null!;
    public string? ServiceDescription { get; set; }
    public PricingType PricingType { get; set; }
    public decimal? BasePrice { get; set; }
    public decimal? CustomPrice { get; set; }
    public decimal EffectivePrice { get; set; }
    public int? EstimatedDurationMins { get; set; }
    public int? ServiceExperienceYears { get; set; }

    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public string? CategoryIconUrl { get; set; }
}
