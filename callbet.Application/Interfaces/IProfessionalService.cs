using callbet.Application.DTOs;

namespace callbet.Application.Interfaces;

public interface IProfessionalService
{
    // Task<Guid> RegisterProfessionalAsync(UserDto dto, CancellationToken ct);
    Task<Guid> CreateProfileAsync(ProfessionalProfileDto dto, CancellationToken ct);
    Task<Guid> UploadResumeAsync(ResumeDto dto, CancellationToken ct);
    Task<Guid> AddCertificateAsync(CertificateDto dto, CancellationToken ct);
    Task<Guid> AddPortfolioItemAsync(PortfolioItemDto dto, CancellationToken ct);

    // Assign a service to a professional profile
    Task<Guid> AssignServiceAsync(ProfessionalServiceDto dto, CancellationToken ct);

    // Fetch services offered by a professional
    Task<IEnumerable<object>> GetProfessionalServicesAsync(Guid profileId, CancellationToken ct);

    // Refresh professional profile reputation
    Task<bool> RefreshReputationAsync(Guid userId, CancellationToken ct);

    // Availability Schedule
    Task<Guid> AddAvailabilityScheduleAsync(AvailabilityScheduleDto dto, CancellationToken ct);
    Task<IEnumerable<AvailabilityScheduleDto>> GetAvailabilitySchedulesAsync(Guid profileId, CancellationToken ct);

    // Complete Profile Details
    Task<ProfessionalProfileDetailsDto?> GetProfileDetailsAsync(Guid id, CancellationToken ct);
}
