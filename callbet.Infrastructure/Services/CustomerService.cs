using callbet.Application.DTOs;
using callbet.Application.Interfaces;
using callbet.Domain.Entities;
using callbet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using callbet.Application.Services;

namespace callbet.Infrastructure.Services;

public class CustomerService(CallbetDbContext context) : ICustomerService
{
    private readonly CryptoService _crypto = new();
    public async Task<Guid> RegisterCustomerAsync(UserDto dto, CancellationToken ct)
    {
        var customer = new User
        {
            Id = Guid.NewGuid(),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            PasswordHash = _crypto.HashPassword(dto.PasswordHash),
            Role = UserRole.Customer,
            Status = UserStatus.PendingVerification,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Users.Add(customer);
        await context.SaveChangesAsync(ct);
        return customer.Id;
    }

    public async Task<Guid> RegisterProfessionalAsync(UserDto dto, CancellationToken ct)
    {
        var professional = new User
        {
            Id = Guid.NewGuid(),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            PasswordHash = _crypto.HashPassword(dto.PasswordHash),
            Role = UserRole.Professional,
            Status = UserStatus.PendingVerification,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Users.Add(professional);
        await context.SaveChangesAsync(ct);
        return professional.Id;
    }

    public async Task<UserDto?> GetByEmailAsync(string email, CancellationToken ct)
    {
        return await context.Users
            .Where(u => u.Email == email)
            .Select(u => new UserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Phone = u.Phone,
                Role = u.Role,
                Status = u.Status,
                CreatedAt = u.CreatedAt
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PagedResponse<ProfessionalProfileServiceDto>> GetProfessionalServicesPagedAsync(PagedRequest request, CancellationToken ct)
    {
        var query = context.ProfessionalServices
            .Include(ps => ps.ProfessionalProfile)
                .ThenInclude(p => p.User)
            .Include(ps => ps.Service)
                .ThenInclude(s => s.Category)
            .AsNoTracking();

        // Filter by Service Category if specified
        if (request.CategoryId.HasValue && request.CategoryId.Value != Guid.Empty)
        {
            query = query.Where(ps => ps.Service.CategoryId == request.CategoryId.Value);
        }

        // Search by Service Name or Service Description
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchPattern = $"%{request.Search.Trim()}%";
            query = query.Where(ps => EF.Functions.ILike(ps.Service.Name, searchPattern) ||
                                      (ps.Service.Description != null && EF.Functions.ILike(ps.Service.Description, searchPattern)));
        }

        // Filter by Professional verification status if specified
        if (request.IsVerified.HasValue)
        {
            query = query.Where(ps => ps.ProfessionalProfile.IsVerified == request.IsVerified.Value);
        }

        var totalCount = await query.CountAsync(ct);

        // Apply ordering
        query = (request.OrderBy?.ToLower()) switch
        {
            "price" => request.Descending
                ? query.OrderByDescending(ps => ps.CustomPrice ?? ps.Service.BasePrice ?? 0m)
                : query.OrderBy(ps => ps.CustomPrice ?? ps.Service.BasePrice ?? 0m),
            "rating" => request.Descending
                ? query.OrderByDescending(ps => ps.ProfessionalProfile.OverallRating)
                : query.OrderBy(ps => ps.ProfessionalProfile.OverallRating),
            "experience" => request.Descending
                ? query.OrderByDescending(ps => ps.ExperienceYears ?? ps.ProfessionalProfile.YearsOfExperience)
                : query.OrderBy(ps => ps.ExperienceYears ?? ps.ProfessionalProfile.YearsOfExperience),
            "jobs" => request.Descending
                ? query.OrderByDescending(ps => ps.ProfessionalProfile.CompletedJobsCount)
                : query.OrderBy(ps => ps.ProfessionalProfile.CompletedJobsCount),
            "name" => request.Descending
                ? query.OrderByDescending(ps => ps.Service.Name)
                : query.OrderBy(ps => ps.Service.Name),
            _ => request.Descending
                ? query.OrderByDescending(ps => ps.Service.Name)
                : query.OrderBy(ps => ps.Service.Name)
        };

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(ps => new ProfessionalProfileServiceDto
            {
                Id = ps.Id,
                ProfessionalProfileId = ps.ProfessionalProfileId,
                UserId = ps.ProfessionalProfile.UserId,
                ProfessionalName = ps.ProfessionalProfile.User.FirstName + " " + ps.ProfessionalProfile.User.LastName,
                ProfessionalHeadline = ps.ProfessionalProfile.Headline,
                ProfilePhotoUrl = ps.ProfessionalProfile.User.ProfilePhotoUrl,
                OverallRating = ps.ProfessionalProfile.OverallRating,
                CompletedJobsCount = ps.ProfessionalProfile.CompletedJobsCount,
                IsVerified = ps.ProfessionalProfile.IsVerified,
                ProfessionalExperienceYears = ps.ProfessionalProfile.YearsOfExperience,
                ServiceId = ps.ServiceId,
                ServiceName = ps.Service.Name,
                ServiceDescription = ps.Service.Description,
                PricingType = ps.Service.PricingType,
                BasePrice = ps.Service.BasePrice,
                CustomPrice = ps.CustomPrice,
                EffectivePrice = ps.CustomPrice ?? ps.Service.BasePrice ?? 0m,
                EstimatedDurationMins = ps.Service.EstimatedDurationMins,
                ServiceExperienceYears = ps.ExperienceYears,
                CategoryId = ps.Service.CategoryId,
                CategoryName = ps.Service.Category.Name,
                CategoryIconUrl = ps.Service.Category.IconUrl
            })
            .ToListAsync(ct);

        return new PagedResponse<ProfessionalProfileServiceDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<PagedResponse<ServiceCategory>> GetServiceCategoriesPagedAsync(PagedRequest request, CancellationToken ct)
    {
        var query = context.ServiceCategories.AsNoTracking();

        // Apply search filter on Name or Description
        if (!string.IsNullOrEmpty(request.Search))
        {
            var searchPattern = $"%{request.Search}%";
            query = query.Where(sc => EF.Functions.ILike(sc.Name, searchPattern) ||
                                      (sc.Description != null && EF.Functions.ILike(sc.Description, searchPattern)));
        }

        var totalCount = await query.CountAsync(ct);

        // Apply ordering
        query = request.OrderBy switch
        {
            "Name" => request.Descending ? query.OrderByDescending(sc => sc.Name) : query.OrderBy(sc => sc.Name),
            _ => query.OrderBy(sc => sc.Name) // Default sort
        };

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return new PagedResponse<ServiceCategory>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<IEnumerable<Service>> GetServicesAsync(CancellationToken ct)
    {
        return await context.Services
            .OrderBy(s => s.Name)
            .ToListAsync(ct);
    }
    public Task<bool> VerifyPasswordAsync(string plainText, string hashed)
    {
        bool isValid = _crypto.VerifyPassword(plainText, hashed);
        return Task.FromResult(isValid);
    }

    public async Task<Guid> AddAddressAsync(AddressDto dto, CancellationToken ct)
    {
        var address = new Address
        {
            Id = dto.Id != Guid.Empty ? dto.Id : Guid.NewGuid(),
            UserId = dto.UserId,
            Street = dto.Street,
            City = dto.City,
            Country = dto.Country,
            Label = dto.Label,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude
        };

        context.Addresses.Add(address);
        await context.SaveChangesAsync(ct);
        return address.Id;
    }

    public async Task<IEnumerable<AddressDto>> GetUserAddressesAsync(Guid userId, CancellationToken ct)
    {
        return await context.Addresses
            .Where(a => a.UserId == userId)
            .Select(a => new AddressDto
            {
                Id = a.Id,
                UserId = a.UserId,
                Street = a.Street,
                City = a.City,
                Country = a.Country,
                Label = a.Label,
                Latitude = a.Latitude,
                Longitude = a.Longitude
            })
            .ToListAsync(ct);
    }

    public async Task<Guid> AddFavoriteProfessionalAsync(FavoriteProfessionalDto dto, CancellationToken ct)
    {
        var profile = await context.ProfessionalProfiles
            .FirstOrDefaultAsync(p => p.Id == dto.ProfessionalProfileId || p.UserId == dto.ProfessionalProfileId, ct);
        var targetProfileId = profile != null ? profile.Id : dto.ProfessionalProfileId;

        var existing = await context.FavoriteProfessionals
            .FirstOrDefaultAsync(f => f.CustomerId == dto.CustomerId && f.ProfessionalProfileId == targetProfileId, ct);
        if (existing != null) return existing.Id;

        var fav = new FavoriteProfessional
        {
            Id = dto.Id != Guid.Empty ? dto.Id : Guid.NewGuid(),
            CustomerId = dto.CustomerId,
            ProfessionalProfileId = targetProfileId,
            CreatedAt = DateTime.UtcNow
        };

        context.FavoriteProfessionals.Add(fav);
        await context.SaveChangesAsync(ct);
        return fav.Id;
    }

    public async Task<bool> RemoveFavoriteProfessionalAsync(Guid customerId, Guid professionalProfileId, CancellationToken ct)
    {
        var profile = await context.ProfessionalProfiles
            .FirstOrDefaultAsync(p => p.Id == professionalProfileId || p.UserId == professionalProfileId, ct);
        var targetProfileId = profile != null ? profile.Id : professionalProfileId;

        var fav = await context.FavoriteProfessionals
            .FirstOrDefaultAsync(f => f.CustomerId == customerId && (f.ProfessionalProfileId == targetProfileId || f.Id == professionalProfileId), ct);

        if (fav == null) return false;

        context.FavoriteProfessionals.Remove(fav);
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IEnumerable<object>> GetFavoriteProfessionalsAsync(Guid customerId, CancellationToken ct)
    {
        return await context.FavoriteProfessionals
            .Where(f => f.CustomerId == customerId)
            .Join(context.ProfessionalProfiles,
                  f => f.ProfessionalProfileId,
                  p => p.Id,
                  (f, p) => new { f, p })
            .Join(context.Users,
                  fp => fp.p.UserId,
                  u => u.Id,
                  (fp, u) => new
                  {
                      fp.f.Id,
                      fp.f.CustomerId,
                      fp.f.ProfessionalProfileId,
                      fp.f.CreatedAt,
                      ProfessionalUserId = u.Id,
                      ProfessionalName = u.FirstName + " " + u.LastName,
                      fp.p.Headline,
                      fp.p.Bio,
                      fp.p.OverallRating,
                      fp.p.CompletedJobsCount,
                      fp.p.IsVerified,
                      u.ProfilePhotoUrl
                  })
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Guid> AddFavoriteServiceAsync(FavoriteServiceDto dto, CancellationToken ct)
    {
        var existing = await context.FavoriteServices
            .FirstOrDefaultAsync(f => f.CustomerId == dto.CustomerId && f.ServiceId == dto.ServiceId, ct);
        if (existing != null) return existing.Id;

        var fav = new FavoriteService
        {
            Id = dto.Id != Guid.Empty ? dto.Id : Guid.NewGuid(),
            CustomerId = dto.CustomerId,
            ServiceId = dto.ServiceId,
            CreatedAt = DateTime.UtcNow
        };

        context.FavoriteServices.Add(fav);
        await context.SaveChangesAsync(ct);
        return fav.Id;
    }

    public async Task<bool> RemoveFavoriteServiceAsync(Guid customerId, Guid serviceId, CancellationToken ct)
    {
        var fav = await context.FavoriteServices
            .FirstOrDefaultAsync(f => f.CustomerId == customerId && (f.ServiceId == serviceId || f.Id == serviceId), ct);

        if (fav == null) return false;

        context.FavoriteServices.Remove(fav);
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IEnumerable<object>> GetFavoriteServicesAsync(Guid customerId, CancellationToken ct)
    {
        return await context.FavoriteServices
            .Where(f => f.CustomerId == customerId)
            .Join(context.Services.Include(s => s.Category),
                  f => f.ServiceId,
                  s => s.Id,
                  (f, s) => new
                  {
                      f.Id,
                      f.CustomerId,
                      f.ServiceId,
                      f.CreatedAt,
                      ServiceName = s.Name,
                      ServiceDescription = s.Description,
                      s.BasePrice,
                      s.PricingType,
                      s.CategoryId,
                      CategoryName = s.Category.Name,
                      s.Category.IconUrl
                  })
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<CustomerProfileDto?> GetCustomerProfileAsync(Guid userId, CancellationToken ct)
    {
        var user = await context.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user == null) return null;

        return new CustomerProfileDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            Phone = user.Phone,
            ProfilePhotoUrl = user.ProfilePhotoUrl,
            Role = user.Role.ToString(),
            Status = user.Status.ToString(),
            CreatedAt = user.CreatedAt,
            Addresses = user.Addresses.Select(a => new AddressDto
            {
                Id = a.Id,
                UserId = a.UserId,
                Label = a.Label,
                Country = a.Country,
                City = a.City,
                Street = a.Street,
                Latitude = a.Latitude,
                Longitude = a.Longitude
            }).ToList()
        };
    }

    public async Task<bool> UpdateCustomerProfileAsync(Guid userId, UpdateCustomerProfileDto dto, CancellationToken ct)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null) return false;

        if (!string.IsNullOrWhiteSpace(dto.FirstName)) user.FirstName = dto.FirstName;
        if (!string.IsNullOrWhiteSpace(dto.LastName)) user.LastName = dto.LastName;
        if (!string.IsNullOrWhiteSpace(dto.Phone)) user.Phone = dto.Phone;
        if (dto.ProfilePhotoUrl != null) user.ProfilePhotoUrl = dto.ProfilePhotoUrl;

        user.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
        return true;
    }
}
