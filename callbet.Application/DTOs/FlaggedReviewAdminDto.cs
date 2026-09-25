using System;

namespace callbet.Application.DTOs;

public class FlaggedReviewAdminDto
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public Guid ReviewerId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public Guid RevieweeId { get; set; }
    public string ProfessionalName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string? FlagReason { get; set; }
    public DateTime? FlagDate { get; set; }
    public string Status { get; set; } = "Flagged";
    public DateTime CreatedAt { get; set; }
}
