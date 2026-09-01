using System;

namespace callbet.Application.DTOs
{
    public class FavoriteProfessionalDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid ProfessionalProfileId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}