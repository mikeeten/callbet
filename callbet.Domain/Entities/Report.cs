using System;

namespace callbet.Domain.Entities
{
    public class Report
    {
        public Guid Id { get; set; }
        public Guid ReporterUserId { get; set; }
        public Guid ReportedUserId { get; set; }
        public string Reason { get; set; } = null!;
        public string? Details { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsResolved { get; set; } = false;
    }
}