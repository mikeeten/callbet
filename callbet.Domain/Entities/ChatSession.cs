using System;
using System.Collections.Generic;

namespace callbet.Domain.Entities
{
    public class ChatSession
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid ProfessionalId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}