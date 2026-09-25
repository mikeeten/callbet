using System;
using callbet.Domain.Entities;

namespace callbet.Application.DTOs;

public class ProfessionalProfileAdminDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? ProfilePhotoUrl { get; set; }
    public string? Headline { get; set; }
    public string? Bio { get; set; }
    public int ServiceRadiusKm { get; set; }
    public int YearsOfExperience { get; set; }
    public decimal OverallRating { get; set; }
    public int CompletedJobsCount { get; set; }
    public bool IsVerified { get; set; }
    public UserStatus UserStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
