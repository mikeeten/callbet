using System;

namespace callbet.Domain.Entities
{
    public class Certificate
    {
        public Guid Id { get; set; }
        public Guid ProfessionalProfileId { get; set; }
        public string Title { get; set; } = null!;
        public string Organization { get; set; } = null!;
        public DateTime IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string DocumentImageUrl { get; set; } = null!;

        public ProfessionalProfile ProfessionalProfile { get; set; } = null!;
    }
}