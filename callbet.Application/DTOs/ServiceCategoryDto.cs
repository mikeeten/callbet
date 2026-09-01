using System;
using System.Collections.Generic;

namespace callbet.Application.DTOs
{
    public class ServiceCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? IconUrl { get; set; }
        public string? Description { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public ICollection<ServiceCategoryDto> SubCategories { get; set; } = new List<ServiceCategoryDto>();
    }
}