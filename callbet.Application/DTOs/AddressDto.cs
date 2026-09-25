using System;

namespace callbet.Application.DTOs
{
    public class AddressDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public int? NeighborhoodId { get; set; }
        
        public int? SubCityId { get; set; }
        public string? SubCityName { get; set; }
        public string? NeighborhoodName { get; set; }
        
        public string? Label { get; set; }
        public string? Landmark { get; set; }
        public string? PrimaryPhone { get; set; }
        public string? Street { get; set; }
        public string City { get; set; } = "Addis Ababa";
        public string Country { get; set; } = "Ethiopia";
        
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}