using System;
using System.Collections.Generic;

namespace callbet.Application.DTOs;

public class UserVerificationProfileDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public string VerificationBadge { get; set; } = string.Empty;
    public DateTime? VerificationDate { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    
    public Guid? ProfessionalProfileId { get; set; }
    public string? Headline { get; set; }
    public decimal OverallRating { get; set; }
    public int CompletedJobsCount { get; set; }
    public int YearsOfExperience { get; set; }

    public IEnumerable<VerificationItemDto> Documents { get; set; } = new List<VerificationItemDto>();
    public IEnumerable<CertificateDto> Certificates { get; set; } = new List<CertificateDto>();
}

public class VerificationItemDto
{
    public Guid Id { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentUrl { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public DateTime? VerificationDate { get; set; }
}
