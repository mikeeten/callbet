using System;

namespace callbet.Application.DTOs
{
    public class ReviewReplyDto
    {
        public Guid Id { get; set; }
        public Guid ReviewId { get; set; }
        public Guid ReplierId { get; set; }
        public string Comment { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}