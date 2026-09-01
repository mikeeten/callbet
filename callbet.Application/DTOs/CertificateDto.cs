using System;

namespace callbet.Application.DTOs
{
    public class CertificateDto
    {
        public Guid Id { get; set; }
        public Guid ProfessionalProfileId { get; set; }
        public string Title { get; set; } = null!;
        public string Organization { get; set; } = null!;
        public DateTime IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string DocumentImageUrl { get; set; } = null!;
    }
}