using callbet.Application.DTOs;
using callbet.Domain.Entities;

namespace callbet.Application.Interfaces;

public interface IAdminService
{
    // User management
    Task<IEnumerable<object>> GetPendingProfessionalsAsync(CancellationToken ct);
    Task<Guid> ApproveProfessionalAsync(Guid userId, CancellationToken ct);
    Task<Guid> SuspendProfessionalAsync(Guid userId, CancellationToken ct);
    Task<Guid> DeleteUserAsync(Guid userId, CancellationToken ct);
    Task<Guid> RegisterAdminAsync(UserDto dto, CancellationToken ct);

    Task<PagedResponse<UserResponseDto>> GetUsersAsync(PagedRequest request, CancellationToken ct);


    // Service categories
    Task<Guid> CreateServiceCategoryAsync(ServiceCategoryDto dto, CancellationToken ct);
    Task<IEnumerable<ServiceCategory>> GetServiceCategoriesAsync(CancellationToken ct);
    // Task<PagedResponse<ServiceCategory>> GetServiceCategoriesPagedAsync(PagedRequest request, CancellationToken ct); // Removed

    // Services
    Task<Guid> CreateServiceAsync(ServiceDto dto, CancellationToken ct);
    Task<IEnumerable<Service>> GetServicesByCategoryAsync(Guid categoryId, CancellationToken ct);

    // Professional services
    Task<Guid> AssignServiceAsync(ProfessionalServiceDto dto, CancellationToken ct);
    Task<IEnumerable<object>> GetProfessionalServicesAsync(Guid profileId, CancellationToken ct);

    // Admin Logs / Audit Trail
    Task<PagedResponse<AdminLogDto>> GetAdminLogsPagedAsync(PagedRequest request, CancellationToken ct);
}
