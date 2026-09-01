using System;

namespace callbet.Domain.Entities
{
    public class FavoriteService
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid ServiceId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}