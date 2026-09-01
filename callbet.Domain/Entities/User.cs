using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace callbet.Domain.Entities
{
    public class User : IdentityUser<Guid>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? ProfilePhotoUrl { get; set; }
        public UserRole Role { get; set; }
        public UserStatus Status { get; set; } = UserStatus.PendingVerification;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ProfessionalProfile? ProfessionalProfile { get; set; }
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
    }
}