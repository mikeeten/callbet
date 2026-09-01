using System;

namespace callbet.Domain.Entities
{
    public class Address
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? Label { get; set; }
        public string Country { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Street { get; set; } = null!;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public User User { get; set; } = null!;
    }
}