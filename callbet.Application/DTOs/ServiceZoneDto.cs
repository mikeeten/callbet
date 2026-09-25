using System;

namespace callbet.Application.DTOs;

public class ServiceZoneDto
{
    public int Id { get; set; }
    public int SubCityId { get; set; }
    public string SubCity { get; set; } = string.Empty;
    public string Neighborhood { get; set; } = string.Empty;
    public int ActiveProsCount { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; }
}

public class CreateServiceZoneDto
{
    public string? SubCity { get; set; }
    public int? SubCityId { get; set; }
    public string Neighborhood { get; set; } = string.Empty;
}
