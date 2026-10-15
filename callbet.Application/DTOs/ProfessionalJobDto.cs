using System;

namespace callbet.Application.DTOs;

public class ProfessionalJobDto
{
    public Guid Id { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }
    public Guid CustomerId { get; set; }
    public Guid ProfessionalId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal NetPayout { get; set; }
    public decimal PlatformFee { get; set; }
    public string ScheduledDate { get; set; } = string.Empty;
    public DateTime? ScheduledDateTime { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? Neighborhood { get; set; }
    public string? SubCity { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
