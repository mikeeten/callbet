using System;

namespace callbet.Application.DTOs
{
    public class FavoriteServiceDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid ServiceId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}