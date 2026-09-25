using System;

namespace callbet.Application.DTOs
{
    public class ReportAbuseDto
    {
        public Guid? Id { get; set; }
        public Guid? ReporterUserId { get; set; }
        public Guid? ReportedUserId { get; set; }
        public string? Category { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool IsResolved { get; set; }
    }
}
