using System;

namespace callbet.Domain.Entities
{
    public class Address
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public int? NeighborhoodId { get; set; }
        
        public string? Label { get; set; }
        public string? Landmark { get; set; }
        public string? PrimaryPhone { get; set; }
        public string? Street { get; set; }
        public string City { get; set; } = "Addis Ababa";
        public string Country { get; set; } = "Ethiopia";
        
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
        public Neighborhood? Neighborhood { get; set; }
    }
}