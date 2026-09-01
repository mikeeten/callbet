using System;
using System.Collections.Generic;

namespace callbet.Domain.Entities
{
    public class Job
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid ProfessionalId { get; set; }
        public Guid ServiceId { get; set; }
        public string Description { get; set; } = null!;
        public Guid AddressId { get; set; }
        public DateTime ScheduledDateTime { get; set; }
        public int? EstimatedDurationMins { get; set; }
        public decimal Price { get; set; }
        public JobStatus Status { get; set; } = JobStatus.Draft;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}