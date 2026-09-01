namespace callbet.Application.Interfaces;
using callbet.Domain.Entities;
using callbet.Application.DTOs;

public interface ICustomerService
{
    Task<Guid> RegisterCustomerAsync(UserDto dto, CancellationToken ct);
    Task<Guid> RegisterProfessionalAsync(UserDto dto, CancellationToken ct);
    Task<UserDto?> GetByEmailAsync(string email, CancellationToken ct);
    Task<PagedResponse<ProfessionalProfileServiceDto>> GetProfessionalServicesPagedAsync(PagedRequest request, CancellationToken ct);
    Task<PagedResponse<ServiceCategory>> GetServiceCategoriesPagedAsync(PagedRequest request, CancellationToken ct);
    Task<IEnumerable<Service>> GetServicesAsync(CancellationToken ct);
    Task<bool> VerifyPasswordAsync(string plainText, string hashed);
    Task<Guid> AddAddressAsync(AddressDto dto, CancellationToken ct);
    Task<IEnumerable<AddressDto>> GetUserAddressesAsync(Guid userId, CancellationToken ct);

    // Favorites / Bookmarks
    Task<Guid> AddFavoriteProfessionalAsync(FavoriteProfessionalDto dto, CancellationToken ct);
    Task<bool> RemoveFavoriteProfessionalAsync(Guid customerId, Guid professionalProfileId, CancellationToken ct);
    Task<IEnumerable<object>> GetFavoriteProfessionalsAsync(Guid customerId, CancellationToken ct);
    Task<Guid> AddFavoriteServiceAsync(FavoriteServiceDto dto, CancellationToken ct);
    Task<bool> RemoveFavoriteServiceAsync(Guid customerId, Guid serviceId, CancellationToken ct);
    Task<IEnumerable<object>> GetFavoriteServicesAsync(Guid customerId, CancellationToken ct);

    // Customer Personal Profile Management
    Task<CustomerProfileDto?> GetCustomerProfileAsync(Guid userId, CancellationToken ct);
    Task<bool> UpdateCustomerProfileAsync(Guid userId, UpdateCustomerProfileDto dto, CancellationToken ct);
}
