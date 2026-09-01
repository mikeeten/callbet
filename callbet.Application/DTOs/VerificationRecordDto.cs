using System;

namespace callbet.Application.DTOs
{
    public class VerificationRecordDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string DocumentType { get; set; } = null!;
        public string DocumentUrl { get; set; } = null!;
        public bool IsVerified { get; set; }
        public DateTime? VerificationDate { get; set; }
    }
}