using callbet.Application.DTOs;
using callbet.Application.Interfaces;
using callbet.Domain.Entities;
using callbet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace callbet.Infrastructure.Services;

public class AdminService(CallbetDbContext context) : IAdminService
{
   
   public async Task<PagedResponse<UserResponseDto>> GetUsersAsync(PagedRequest request, CancellationToken ct)
   {
        // Start with a query that filters out Admin users
        var query = context.Users
            .AsNoTracking()
            .Where(u => u.Role != UserRole.Admin);

        // Apply search filter on FirstName, LastName, or Email
        if (!string.IsNullOrEmpty(request.Search))
        {
            var searchPattern = $"%{request.Search}%";
            query = query.Where(u => EF.Functions.ILike(u.FirstName, searchPattern) ||
                                     EF.Functions.ILike(u.LastName, searchPattern) ||
                                     EF.Functions.ILike(u.Email, searchPattern));
        }

        var totalCount = await query.CountAsync(ct);

        // Apply ordering
        query = request.OrderBy switch
        {
            "FirstName" => request.Descending ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName),
            "LastName" => request.Descending ? query.OrderByDescending(u => u.LastName) : query.OrderBy(u => u.LastName),
            _ => query.OrderByDescending(u => u.CreatedAt) // Default sort
        };

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UserResponseDto
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
            .ToListAsync(ct);

        return new PagedResponse<UserResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

 
    // User management
    public async Task<IEnumerable<object>> GetPendingProfessionalsAsync(CancellationToken ct)
    {
        return await context.Users
            .Where(u => u.Role == UserRole.Professional && u.Status == UserStatus.PendingVerification)
            .Join(context.ProfessionalProfiles,
                  u => u.Id,
                  pp => pp.UserId,
                  (u, pp) => new {
                      u.Id,
                      u.FirstName,
                      u.LastName,
                      pp.Headline,
                      pp.Bio,
                      u.Status
                  })
            .ToListAsync(ct);
    }

    public async Task<Guid> ApproveProfessionalAsync(Guid userId, CancellationToken ct)
    {
        var user = await context.Users.FindAsync(new object[] { userId }, ct);
        if (user == null) return Guid.Empty;

        user.Status = UserStatus.Active;
        user.UpdatedAt = DateTime.UtcNow;

        var profile = await context.ProfessionalProfiles.FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (profile != null)
        {
            profile.IsVerified = true;
            profile.UpdatedAt = DateTime.UtcNow;
        }

        // Notification of approval to professional
        context.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Message = "Congratulations! Your professional profile has been approved and is now active.",
            Link = "/profile",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        // Audit Log
        context.AdminLogs.Add(new AdminLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = Guid.Empty,
            Action = "Approved professional profile",
            TargetEntity = "ProfessionalProfile",
            TargetEntityId = profile?.Id ?? userId,
            Timestamp = DateTime.UtcNow
        });

        await context.SaveChangesAsync(ct);
        return user.Id;
    }

    public async Task<Guid> SuspendProfessionalAsync(Guid userId, CancellationToken ct)
    {
        var user = await context.Users.FindAsync(new object[] { userId }, ct);
        if (user == null) return Guid.Empty;

        user.Status = UserStatus.Suspended;
        user.UpdatedAt = DateTime.UtcNow;

        // Audit Log
        context.AdminLogs.Add(new AdminLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = Guid.Empty,
            Action = "Suspended professional",
            TargetEntity = "User",
            TargetEntityId = user.Id,
            Timestamp = DateTime.UtcNow
        });

        await context.SaveChangesAsync(ct);
        return user.Id;
    }

    public async Task<Guid> DeleteUserAsync(Guid userId, CancellationToken ct)
    {
        var user = await context.Users.FindAsync(new object[] { userId }, ct);
        if (user == null) return Guid.Empty;

        context.Users.Remove(user);
        await context.SaveChangesAsync(ct);
        return user.Id;
    }

    public async Task<Guid> RegisterAdminAsync(UserDto dto, CancellationToken ct)
    {
        var crypto = new callbet.Application.Services.CryptoService();
        var admin = new User
        {
            Id = Guid.NewGuid(),
            UserName = dto.Email,
            NormalizedUserName = dto.Email.ToUpperInvariant(),
            Email = dto.Email,
            NormalizedEmail = dto.Email.ToUpperInvariant(),
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Phone = dto.Phone,
            PasswordHash = crypto.HashPassword(dto.PasswordHash),
            Role = UserRole.Admin,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Users.Add(admin);

        // Ensure Admin role exists and assign user to Admin role in AspNetUserRoles
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin", ct);
        if (adminRole == null)
        {
            adminRole = new Microsoft.AspNetCore.Identity.IdentityRole<Guid>("Admin")
            {
                NormalizedName = "ADMIN"
            };
            context.Roles.Add(adminRole);
            await context.SaveChangesAsync(ct);
        }

        context.UserRoles.Add(new Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>
        {
            UserId = admin.Id,
            RoleId = adminRole.Id
        });

        await context.SaveChangesAsync(ct);
        return admin.Id;
    }

    // Service categories
    public async Task<Guid> CreateServiceCategoryAsync(ServiceCategoryDto dto, CancellationToken ct)
    {
        var category = new ServiceCategory
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description
        };

        context.ServiceCategories.Add(category);
        await context.SaveChangesAsync(ct);
        return category.Id;
    }

    public async Task<IEnumerable<ServiceCategory>> GetServiceCategoriesAsync(CancellationToken ct)
    {
        return await context.ServiceCategories
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    // Services
    public async Task<Guid> CreateServiceAsync(ServiceDto dto, CancellationToken ct)
    {
        var service = new Service
        {
            Id = Guid.NewGuid(),
            CategoryId = dto.CategoryId,
            Name = dto.Name,
            Description = dto.Description,
            PricingType = dto.PricingType,
            BasePrice = dto.BasePrice,
            EstimatedDurationMins = dto.EstimatedDurationMins
        };

        context.Services.Add(service);
        await context.SaveChangesAsync(ct);
        return service.Id;
    }

    public async Task<IEnumerable<Service>> GetServicesByCategoryAsync(Guid categoryId, CancellationToken ct)
    {
        return await context.Services
            .Where(s => s.CategoryId == categoryId)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Service>> GetServicesAsync(CancellationToken ct)
    {
        return await context.Services
            .OrderBy(s => s.Name)
            .ToListAsync(ct);
    }

    // Professional services
    public async Task<Guid> AssignServiceAsync(ProfessionalServiceDto dto, CancellationToken ct)
    {
        var ps = new callbet.Domain.Entities.ProfessionalService
        {
            Id = Guid.NewGuid(),
            ProfessionalProfileId = dto.ProfessionalProfileId,
            ServiceId = dto.ServiceId,
            CustomPrice = dto.CustomPrice,
            ExperienceYears = dto.ExperienceYears
        };

        context.ProfessionalServices.Add(ps);
        await context.SaveChangesAsync(ct);
        return ps.Id;
    }

    public async Task<IEnumerable<object>> GetProfessionalServicesAsync(Guid profileId, CancellationToken ct)
    {
        return await context.ProfessionalServices
            .Where(ps => ps.ProfessionalProfileId == profileId)
            .Join(context.Services,
                  ps => ps.ServiceId,
                  s => s.Id,
                  (ps, s) => new {
                      s.Name,
                      ps.CustomPrice,
                      ps.ExperienceYears
                  })
            .ToListAsync(ct);
    }

    public async Task<PagedResponse<AdminLogDto>> GetAdminLogsPagedAsync(PagedRequest request, CancellationToken ct)
    {
        var query = context.AdminLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = $"%{request.Search.Trim()}%";
            query = query.Where(l => EF.Functions.ILike(l.Action, search) ||
                                     (l.TargetEntity != null && EF.Functions.ILike(l.TargetEntity, search)));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(l => l.Timestamp)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(l => new AdminLogDto
            {
                Id = l.Id,
                AdminUserId = l.AdminUserId,
                Action = l.Action,
                TargetEntity = l.TargetEntity,
                TargetEntityId = l.TargetEntityId,
                Timestamp = l.Timestamp
            })
            .ToListAsync(ct);

        return new PagedResponse<AdminLogDto>
        {
            Items = items,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
