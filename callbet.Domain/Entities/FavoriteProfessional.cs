using System;

namespace callbet.Domain.Entities
{
    public class FavoriteProfessional
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid ProfessionalProfileId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}