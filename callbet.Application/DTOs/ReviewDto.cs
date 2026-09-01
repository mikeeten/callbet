using System;

namespace callbet.Application.DTOs
{
    public class ReviewDto
    {
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public Guid ReviewerId { get; set; }
        public Guid RevieweeId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string? CustomerName { get; set; }
        public DateTime CreatedAt { get; set; }
        public ReviewReplyDto? Reply { get; set; }
    }
}