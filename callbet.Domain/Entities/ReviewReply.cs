using System;

namespace callbet.Domain.Entities
{
    public class ReviewReply
    {
        public Guid Id { get; set; }
        public Guid ReviewId { get; set; }
        public Guid ReplierId { get; set; }
        public string Comment { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}