using System;

namespace callbet.Domain.Entities
{
    public class PortfolioItem
    {
        public Guid Id { get; set; }
        public Guid ProfessionalProfileId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string ImageUrl { get; set; } = null!;
        public DateTime DateCompleted { get; set; }
    }
}