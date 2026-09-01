using System;
using callbet.Domain.Entities;

namespace callbet.Application.DTOs
{
    public class ServiceDto
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; } 
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public PricingType PricingType { get; set; }
        public decimal? BasePrice { get; set; }
        public int? EstimatedDurationMins { get; set; }
    }
}