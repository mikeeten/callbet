using System;

namespace callbet.Application.DTOs;

public class AdminReportDto
{
    public Guid Id { get; set; }
    public Guid ReporterUserId { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public string ReporterEmail { get; set; } = string.Empty;
    public string ReporterRole { get; set; } = "Customer";

    public Guid? ReportedUserId { get; set; }
    public string? ReportedName { get; set; }
    public string? ReportedEmail { get; set; }
    public string? ReportedRole { get; set; }

    public string Reason { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsResolved { get; set; }
}
