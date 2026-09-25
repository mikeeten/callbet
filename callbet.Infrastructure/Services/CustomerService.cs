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
        // Ensure any new catalog services in database are linked to active verified professionals
        var unlinkedServices = await (from s in context.Services
                                      where !context.ProfessionalServices.Any(ps => ps.ServiceId == s.Id)
                                      select s).ToListAsync(ct);

        if (unlinkedServices.Count > 0)
        {
            var verifiedPros = await context.ProfessionalProfiles
                .Include(p => p.User)
                .Where(p => p.IsVerified && p.User.Status == UserStatus.Active)
                .OrderByDescending(p => p.OverallRating)
                .Take(3)
                .ToListAsync(ct);

            if (verifiedPros.Count > 0)
            {
                foreach (var s in unlinkedServices)
                {
                    foreach (var pro in verifiedPros)
                    {
                        context.ProfessionalServices.Add(new callbet.Domain.Entities.ProfessionalService
                        {
                            Id = Guid.NewGuid(),
                            ProfessionalProfileId = pro.Id,
                            ServiceId = s.Id,
                            CustomPrice = s.BasePrice,
                            ExperienceYears = Math.Max(1, pro.YearsOfExperience)
                        });
                    }
                }
                await context.SaveChangesAsync(ct);
            }
        }

        var query = context.ProfessionalServices
            .Include(ps => ps.ProfessionalProfile)
                .ThenInclude(p => p.User)
                    .ThenInclude(u => u.Addresses)
                        .ThenInclude(a => a.Neighborhood)
                            .ThenInclude(n => n!.SubCity)
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

        // Only active, verified professional profiles are visible to customers
        query = query.Where(ps => ps.ProfessionalProfile.IsVerified && ps.ProfessionalProfile.User.Status == UserStatus.Active);

        // Filter by Neighborhood or SubCity if specified
        if (request.NeighborhoodId.HasValue && request.NeighborhoodId.Value > 0)
        {
            query = query.Where(ps => ps.ProfessionalProfile.User.Addresses.Any(a => a.NeighborhoodId == request.NeighborhoodId.Value));
        }
        else if (request.SubCityId.HasValue && request.SubCityId.Value > 0)
        {
            query = query.Where(ps => ps.ProfessionalProfile.User.Addresses.Any(a => a.Neighborhood != null && a.Neighborhood.SubCityId == request.SubCityId.Value));
        }

        // Check if customer provided GPS Coordinates for nearby proximity search
        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            var custLat = request.Latitude.Value;
            var custLon = request.Longitude.Value;
            var maxRadius = request.RadiusKm ?? 25.0; // Default 25km search radius

            // Load candidate services matching category, search, and verification filters
            var rawList = await query.ToListAsync(ct);

            // Compute distance and filter within proximity & professional service radius
            var nearbyItems = rawList
                .Select(ps =>
                {
                    var addr = ps.ProfessionalProfile.User.Addresses.FirstOrDefault(a => a.Latitude.HasValue && a.Longitude.HasValue)
                               ?? ps.ProfessionalProfile.User.Addresses.FirstOrDefault();

                    double? dist = null;
                    if (addr?.Latitude != null && addr?.Longitude != null)
                    {
                        dist = GeoCalculator.CalculateDistanceKm(custLat, custLon, (double)addr.Latitude.Value, (double)addr.Longitude.Value);
                    }

                    var locName = addr != null
                        ? (addr.Neighborhood != null
                            ? $"{addr.Neighborhood.Name}, {addr.Neighborhood.SubCity?.Name ?? addr.City}"
                            : addr.City)
                        : "Addis Ababa";

                    return new
                    {
                        Ps = ps,
                        Distance = dist,
                        Address = addr,
                        LocationName = locName
                    };
                })
                .Where(x => x.Distance == null || (x.Distance <= maxRadius && x.Distance <= x.Ps.ProfessionalProfile.ServiceRadiusKm))
                .ToList();

            var totalNearby = nearbyItems.Count;

            // Sort based on OrderBy
            IEnumerable<dynamic> sorted = (request.OrderBy?.ToLower()) switch
            {
                "distance" => request.Descending ? nearbyItems.OrderByDescending(x => x.Distance ?? 9999) : nearbyItems.OrderBy(x => x.Distance ?? 9999),
                "price" => request.Descending ? nearbyItems.OrderByDescending(x => (decimal)(x.Ps.CustomPrice ?? x.Ps.Service.BasePrice ?? 0m)) : nearbyItems.OrderBy(x => (decimal)(x.Ps.CustomPrice ?? x.Ps.Service.BasePrice ?? 0m)),
                "rating" => request.Descending ? nearbyItems.OrderByDescending(x => (decimal)x.Ps.ProfessionalProfile.OverallRating) : nearbyItems.OrderBy(x => (decimal)x.Ps.ProfessionalProfile.OverallRating),
                "experience" => request.Descending ? nearbyItems.OrderByDescending(x => (int)(x.Ps.ExperienceYears ?? x.Ps.ProfessionalProfile.YearsOfExperience)) : nearbyItems.OrderBy(x => (int)(x.Ps.ExperienceYears ?? x.Ps.ProfessionalProfile.YearsOfExperience)),
                "jobs" => request.Descending ? nearbyItems.OrderByDescending(x => (int)x.Ps.ProfessionalProfile.CompletedJobsCount) : nearbyItems.OrderBy(x => (int)x.Ps.ProfessionalProfile.CompletedJobsCount),
                "name" => request.Descending ? nearbyItems.OrderByDescending(x => (string)x.Ps.Service.Name) : nearbyItems.OrderBy(x => (string)x.Ps.Service.Name),
                _ => nearbyItems.OrderBy(x => x.Distance ?? 9999) // Default: Nearest first
            };

            var pagedItems = sorted
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new ProfessionalProfileServiceDto
                {
                    Id = x.Ps.Id,
                    ProfessionalProfileId = x.Ps.ProfessionalProfileId,
                    UserId = x.Ps.ProfessionalProfile.UserId,
                    ProfessionalName = x.Ps.ProfessionalProfile.User.FirstName + " " + x.Ps.ProfessionalProfile.User.LastName,
                    ProfessionalHeadline = x.Ps.ProfessionalProfile.Headline,
                    ProfilePhotoUrl = x.Ps.ProfessionalProfile.User.ProfilePhotoUrl,
                    OverallRating = x.Ps.ProfessionalProfile.OverallRating,
                    CompletedJobsCount = x.Ps.ProfessionalProfile.CompletedJobsCount,
                    IsVerified = x.Ps.ProfessionalProfile.IsVerified,
                    ProfessionalExperienceYears = x.Ps.ProfessionalProfile.YearsOfExperience,
                    ServiceId = x.Ps.ServiceId,
                    ServiceName = x.Ps.Service.Name,
                    ServiceDescription = x.Ps.Service.Description,
                    PricingType = x.Ps.Service.PricingType,
                    BasePrice = x.Ps.Service.BasePrice,
                    CustomPrice = x.Ps.CustomPrice,
                    EffectivePrice = x.Ps.CustomPrice ?? x.Ps.Service.BasePrice ?? 0m,
                    EstimatedDurationMins = x.Ps.Service.EstimatedDurationMins,
                    ServiceExperienceYears = x.Ps.ExperienceYears,
                    CategoryId = x.Ps.Service.CategoryId,
                    CategoryName = x.Ps.Service.Category.Name,
                    CategoryIconUrl = x.Ps.Service.Category.IconUrl,
                    DistanceKm = x.Distance,
                    ServiceRadiusKm = x.Ps.ProfessionalProfile.ServiceRadiusKm,
                    LocationName = x.LocationName,
                    Latitude = x.Address?.Latitude,
                    Longitude = x.Address?.Longitude
                })
                .ToList();

            return new PagedResponse<ProfessionalProfileServiceDto>
            {
                Items = pagedItems,
                TotalCount = totalNearby,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        // Standard Non-GPS Query
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
                CategoryIconUrl = ps.Service.Category.IconUrl,
                DistanceKm = null,
                ServiceRadiusKm = ps.ProfessionalProfile.ServiceRadiusKm,
                LocationName = ps.ProfessionalProfile.User.Addresses.Select(a => a.Neighborhood != null ? a.Neighborhood.Name + ", " + (a.Neighborhood.SubCity != null ? a.Neighborhood.SubCity.Name : a.City) : a.City).FirstOrDefault(),
                Latitude = ps.ProfessionalProfile.User.Addresses.Select(a => a.Latitude).FirstOrDefault(),
                Longitude = ps.ProfessionalProfile.User.Addresses.Select(a => a.Longitude).FirstOrDefault()
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
            NeighborhoodId = dto.NeighborhoodId,
            Label = dto.Label,
            Landmark = dto.Landmark,
            PrimaryPhone = dto.PrimaryPhone,
            Street = dto.Street,
            City = !string.IsNullOrWhiteSpace(dto.City) ? dto.City : "Addis Ababa",
            Country = !string.IsNullOrWhiteSpace(dto.Country) ? dto.Country : "Ethiopia",
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Addresses.Add(address);
        await context.SaveChangesAsync(ct);
        return address.Id;
    }

    public async Task<bool> UpdateAddressAsync(Guid userId, AddressDto dto, CancellationToken ct)
    {
        var address = await context.Addresses
            .FirstOrDefaultAsync(a => a.Id == dto.Id && a.UserId == userId, ct);

        if (address == null) return false;

        if (dto.NeighborhoodId.HasValue) address.NeighborhoodId = dto.NeighborhoodId.Value;
        if (dto.Label != null) address.Label = dto.Label;
        if (dto.Landmark != null) address.Landmark = dto.Landmark;
        if (dto.PrimaryPhone != null) address.PrimaryPhone = dto.PrimaryPhone;
        if (dto.Street != null) address.Street = dto.Street;
        if (!string.IsNullOrWhiteSpace(dto.City)) address.City = dto.City;
        if (!string.IsNullOrWhiteSpace(dto.Country)) address.Country = dto.Country;
        if (dto.Latitude.HasValue) address.Latitude = dto.Latitude;
        if (dto.Longitude.HasValue) address.Longitude = dto.Longitude;

        address.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAddressAsync(Guid userId, Guid addressId, CancellationToken ct)
    {
        var address = await context.Addresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId, ct);

        if (address == null) return false;

        context.Addresses.Remove(address);
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IEnumerable<AddressDto>> GetUserAddressesAsync(Guid userId, CancellationToken ct)
    {
        return await context.Addresses
            .Include(a => a.Neighborhood)
                .ThenInclude(n => n!.SubCity)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AddressDto
            {
                Id = a.Id,
                UserId = a.UserId,
                NeighborhoodId = a.NeighborhoodId,
                NeighborhoodName = a.Neighborhood != null ? a.Neighborhood.Name : null,
                SubCityId = a.Neighborhood != null ? a.Neighborhood.SubCityId : null,
                SubCityName = a.Neighborhood != null && a.Neighborhood.SubCity != null ? a.Neighborhood.SubCity.Name : null,
                Label = a.Label,
                Landmark = a.Landmark,
                PrimaryPhone = a.PrimaryPhone,
                Street = a.Street,
                City = a.City,
                Country = a.Country,
                Latitude = a.Latitude,
                Longitude = a.Longitude,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
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
                      s.EstimatedDurationMins,
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
                .ThenInclude(a => a.Neighborhood)
                    .ThenInclude(n => n!.SubCity)
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
                NeighborhoodId = a.NeighborhoodId,
                NeighborhoodName = a.Neighborhood != null ? a.Neighborhood.Name : null,
                SubCityId = a.Neighborhood != null ? a.Neighborhood.SubCityId : null,
                SubCityName = a.Neighborhood != null && a.Neighborhood.SubCity != null ? a.Neighborhood.SubCity.Name : null,
                Label = a.Label,
                Landmark = a.Landmark,
                PrimaryPhone = a.PrimaryPhone,
                Country = a.Country,
                City = a.City,
                Street = a.Street,
                Latitude = a.Latitude,
                Longitude = a.Longitude,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
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

    public async Task<bool> UpdateProfilePhotoUrlAsync(Guid userId, string photoUrl, CancellationToken ct)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null) return false;

        user.ProfilePhotoUrl = photoUrl;
        user.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IEnumerable<SubCityDto>> GetSubCitiesAsync(CancellationToken ct)
    {
        return await context.SubCities
            .Include(s => s.Neighborhoods)
            .OrderBy(s => s.Name)
            .Select(s => new SubCityDto
            {
                Id = s.Id,
                Name = s.Name,
                NeighborhoodsCount = s.Neighborhoods.Count,
                Neighborhoods = s.Neighborhoods.Select(n => new NeighborhoodDto
                {
                    Id = n.Id,
                    SubCityId = n.SubCityId,
                    SubCityName = s.Name,
                    Name = n.Name
                }).OrderBy(n => n.Name).ToList()
            })
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<NeighborhoodDto>> GetNeighborhoodsBySubCityAsync(int subCityId, CancellationToken ct)
    {
        return await context.Neighborhoods
            .Include(n => n.SubCity)
            .Where(n => n.SubCityId == subCityId)
            .OrderBy(n => n.Name)
            .Select(n => new NeighborhoodDto
            {
                Id = n.Id,
                SubCityId = n.SubCityId,
                SubCityName = n.SubCity.Name,
                Name = n.Name
            })
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<NeighborhoodDto>> GetNeighborhoodsAsync(CancellationToken ct)
    {
        return await context.Neighborhoods
            .Include(n => n.SubCity)
            .OrderBy(n => n.SubCity.Name)
            .ThenBy(n => n.Name)
            .Select(n => new NeighborhoodDto
            {
                Id = n.Id,
                SubCityId = n.SubCityId,
                SubCityName = n.SubCity.Name,
                Name = n.Name
            })
            .ToListAsync(ct);
    }
}
