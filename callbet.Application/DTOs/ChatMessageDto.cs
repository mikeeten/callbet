using System;

namespace callbet.Application.DTOs
{
    public class ChatMessageDto
    {
        public Guid Id { get; set; }
        public Guid ChatSessionId { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string Content { get; set; } = null!;
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
    }
}