using System;
using System.Collections.Generic;

namespace callbet.Application.DTOs
{
    public class ResumeDto
    {
        public Guid Id { get; set; }
        public Guid ProfessionalProfileId { get; set; } 
        public string? Summary { get; set; }
        public string? EducationJson { get; set; }
        public string? ExperienceJson { get; set; }
        public List<string>? Skills { get; set; }
        public List<string>? Languages { get; set; }
        public string? ResumeFileUrl { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}