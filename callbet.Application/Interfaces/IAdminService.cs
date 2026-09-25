using callbet.Application.DTOs;
using callbet.Domain.Entities;

namespace callbet.Application.Interfaces;

public interface IAdminService
{
    // User management
    Task<IEnumerable<object>> GetPendingProfessionalsAsync(CancellationToken ct);
    Task<Guid> ApproveProfessionalAsync(Guid userId, Guid adminUserId = default, CancellationToken ct = default);
    Task<Guid> SuspendProfessionalAsync(Guid userId, Guid adminUserId = default, CancellationToken ct = default);
    Task<Guid> ActivateUserAsync(Guid userId, Guid adminUserId = default, CancellationToken ct = default);
    Task<Guid> DeleteUserAsync(Guid userId, Guid adminUserId = default, CancellationToken ct = default);
    Task<Guid> RegisterAdminAsync(UserDto dto, Guid adminUserId = default, CancellationToken ct = default);

    Task<PagedResponse<UserResponseDto>> GetUsersAsync(PagedRequest request, CancellationToken ct);
    Task<PagedResponse<ProfessionalProfileAdminDto>> GetProfessionalProfilesAsync(PagedRequest request, CancellationToken ct);
    Task<bool> ToggleVerifyProfessionalAsync(Guid userId, Guid adminUserId = default, CancellationToken ct = default);


    // Service categories
    Task<Guid> CreateServiceCategoryAsync(ServiceCategoryDto dto, Guid adminUserId = default, CancellationToken ct = default);
    Task<IEnumerable<ServiceCategory>> GetServiceCategoriesAsync(CancellationToken ct);
    // Task<PagedResponse<ServiceCategory>> GetServiceCategoriesPagedAsync(PagedRequest request, CancellationToken ct); // Removed

    // Services
    Task<Guid> CreateServiceAsync(ServiceDto dto, Guid adminUserId = default, CancellationToken ct = default);
    Task<IEnumerable<Service>> GetServicesByCategoryAsync(Guid categoryId, CancellationToken ct);
    Task<IEnumerable<Service>> GetServicesAsync(CancellationToken ct = default);

    // Professional services
    Task<Guid> AssignServiceAsync(ProfessionalServiceDto dto, Guid adminUserId = default, CancellationToken ct = default);
    Task<IEnumerable<object>> GetProfessionalServicesAsync(Guid profileId, CancellationToken ct);

    // Admin Logs / Audit Trail
    Task<PagedResponse<AdminLogDto>> GetAdminLogsPagedAsync(PagedRequest request, CancellationToken ct);

    // Content Moderation
    Task<IEnumerable<FlaggedReviewAdminDto>> GetFlaggedReviewsAsync(CancellationToken ct = default);
    Task<bool> DismissReviewFlagAsync(Guid reviewId, Guid adminUserId = default, CancellationToken ct = default);

    // Service Zones / Coverage Zones
    Task<IEnumerable<ServiceZoneDto>> GetServiceZonesAsync(CancellationToken ct = default);
    Task<ServiceZoneDto> CreateServiceZoneAsync(CreateServiceZoneDto dto, Guid adminUserId = default, CancellationToken ct = default);
    Task<bool> DeleteServiceZoneAsync(int neighborhoodId, Guid adminUserId = default, CancellationToken ct = default);
}
