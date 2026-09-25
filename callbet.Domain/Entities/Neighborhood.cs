using System;
using System.Collections.Generic;

namespace callbet.Domain.Entities
{
    public class Neighborhood
    {
        public int Id { get; set; }
        public int SubCityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public SubCity SubCity { get; set; } = null!;
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
    }
}
