using System;
using System.Collections.Generic;

namespace callbet.Application.DTOs
{
    public class ChatSessionDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerAvatar { get; set; }
        public Guid ProfessionalId { get; set; }
        public string? ProfessionalName { get; set; }
        public string? ProfessionalAvatar { get; set; }
        public string? LastMessage { get; set; }
        public DateTime? LastMessageTime { get; set; }
        public int UnreadCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<ChatMessageDto> Messages { get; set; } = new List<ChatMessageDto>();
    }
}