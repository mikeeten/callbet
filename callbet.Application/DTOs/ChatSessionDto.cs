using System;
using System.Collections.Generic;

namespace callbet.Application.DTOs
{
    public class ChatSessionDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid ProfessionalId { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<ChatMessageDto> Messages { get; set; } = new List<ChatMessageDto>();
    }
}