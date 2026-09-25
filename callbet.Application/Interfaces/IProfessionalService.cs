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
    Task<bool> UnassignServiceAsync(Guid professionalProfileId, Guid serviceId, CancellationToken ct = default);

    // Fetch services offered by a professional
    Task<IEnumerable<object>> GetProfessionalServicesAsync(Guid profileId, CancellationToken ct);

    // Refresh professional profile reputation
    Task<bool> RefreshReputationAsync(Guid userId, CancellationToken ct);

    // Availability Schedule
    Task<Guid> AddAvailabilityScheduleAsync(AvailabilityScheduleDto dto, CancellationToken ct);
    Task<IEnumerable<AvailabilityScheduleDto>> GetAvailabilitySchedulesAsync(Guid profileId, CancellationToken ct);
    Task<bool> DeleteAvailabilityScheduleAsync(Guid professionalProfileId, Guid scheduleId, CancellationToken ct = default);

    // Complete Profile Details
    Task<ProfessionalProfileDetailsDto?> GetProfileDetailsAsync(Guid id, CancellationToken ct);

    // Profile Dashboard Details
    Task<GetProfessionalProfileDashboardDto?> GetProfessionalProfileDashboardAsync(Guid id, CancellationToken ct);

    // Delete Portfolio and Certificates
    Task<bool> DeleteCertificateAsync(Guid professionalProfileId, Guid certificateId, CancellationToken ct);
    Task<bool> DeletePortfolioItemAsync(Guid professionalProfileId, Guid portfolioItemId, CancellationToken ct);
}
