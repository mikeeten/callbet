using System;
using System.Collections.Generic;
using callbet.Domain.Entities;

namespace callbet.Application.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? ProfilePhotoUrl { get; set; }
        public UserRole Role { get; set; }
        public UserStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string PasswordHash { get; set; } = null!;
        public ProfessionalProfileDto? ProfessionalProfile { get; set; }
        public ICollection<AddressDto> Addresses { get; set; } = new List<AddressDto>();
    }
}