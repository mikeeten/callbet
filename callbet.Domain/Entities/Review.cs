using System;

namespace callbet.Domain.Entities
{
    public class Review
    {
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public Guid ReviewerId { get; set; }
        public Guid RevieweeId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ReviewReply? Reply { get; set; }
    }
}