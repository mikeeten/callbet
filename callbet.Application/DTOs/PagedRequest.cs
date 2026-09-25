namespace callbet.Application.DTOs;

public record PagedRequest
{
    private const int MaxPageSize = 100;
    private int _pageSize = 20;
    public int Page { get; init; } = 1;

    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value < 1 ? 20 : value > MaxPageSize ? MaxPageSize : value;
    }

    public string? Search { get; init; }
    public string OrderBy { get; init; } = "VerificationDate";
    public bool Descending { get; init; }
    public bool? IsVerified { get; init; }
    public Guid? CategoryId { get; init; }

    // User Directory Filtering
    public string? Role { get; init; }
    public string? Status { get; init; }
    public string? CreatedDate { get; init; }

    // Geolocation / Nearby Search Parameters
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public double? RadiusKm { get; init; }
    public int? NeighborhoodId { get; init; }
    public int? SubCityId { get; init; }
}