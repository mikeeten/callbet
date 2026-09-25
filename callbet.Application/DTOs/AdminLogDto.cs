using System;

namespace callbet.Application.DTOs
{
    public class AdminLogDto
    {
        public Guid Id { get; set; }
        public Guid AdminUserId { get; set; }
        public string? AdminName { get; set; }
        public string? AdminEmail { get; set; }
        public string Action { get; set; } = null!;
        public string? TargetEntity { get; set; }
        public Guid? TargetEntityId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}