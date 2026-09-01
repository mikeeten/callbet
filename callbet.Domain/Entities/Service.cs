using System;
using System.Collections.Generic;

namespace callbet.Domain.Entities
{
    public class Service
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public PricingType PricingType { get; set; } = PricingType.Fixed;
        public decimal? BasePrice { get; set; }
        public int? EstimatedDurationMins { get; set; }

        public ServiceCategory Category { get; set; } = null!;
        public ICollection<ProfessionalService> ProfessionalServices { get; set; } = new List<ProfessionalService>();
    }
}