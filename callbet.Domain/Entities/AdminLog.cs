using System;

namespace callbet.Domain.Entities
{
    public class AdminLog
    {
        public Guid Id { get; set; }
        public Guid AdminUserId { get; set; }
        public string Action { get; set; } = null!;
        public string? TargetEntity { get; set; }
        public Guid? TargetEntityId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}