using System;
using System.Collections.Generic;

namespace callbet.Domain.Entities
{
    public class ServiceCategory
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? IconUrl { get; set; }
        public string? Description { get; set; }
        public Guid? ParentCategoryId { get; set; }

        public ServiceCategory? ParentCategory { get; set; }
        public ICollection<ServiceCategory> SubCategories { get; set; } = new List<ServiceCategory>();
        public ICollection<Service> Services { get; set; } = new List<Service>();
    }
}