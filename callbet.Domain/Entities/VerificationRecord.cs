using System;

namespace callbet.Domain.Entities
{
    public class VerificationRecord
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string DocumentType { get; set; } = null!;
        public string DocumentUrl { get; set; } = null!;
        public bool IsVerified { get; set; } = false;
        public DateTime? VerificationDate { get; set; }
        // public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}