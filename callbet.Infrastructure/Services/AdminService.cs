using callbet.Application.DTOs;
using callbet.Application.Interfaces;
using callbet.Domain.Entities;
using callbet.Infrastructure.Hubs;
using callbet.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
namespace callbet.Infrastructure.Services;

public class AdminService(
    CallbetDbContext context,
    IHubContext<ChatHub, IChatHubClient> hubContext) : IAdminService
{
   
   public async Task<PagedResponse<UserResponseDto>> GetUsersAsync(PagedRequest request, CancellationToken ct)
   {
        // Start with a query that filters out Admin users
        var query = context.Users
            .AsNoTracking()
            .Where(u => u.Role != UserRole.Admin);

        // Apply Role filter
        if (!string.IsNullOrWhiteSpace(request.Role) && !request.Role.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            if (Enum.TryParse<UserRole>(request.Role, true, out var roleEnum))
            {
                query = query.Where(u => u.Role == roleEnum);
            }
        }

        // Apply Status filter
        if (!string.IsNullOrWhiteSpace(request.Status) && !request.Status.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            var statusStr = request.Status.Trim();
            if (statusStr.Equals("Pending", StringComparison.OrdinalIgnoreCase) ||
                statusStr.Equals("PendingVerification", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(u => u.Status == UserStatus.PendingVerification);
            }
            else if (Enum.TryParse<UserStatus>(statusStr, true, out var statusEnum))
            {
                query = query.Where(u => u.Status == statusEnum);
            }
        }

        // Apply search filter on FirstName, LastName, Email, or Phone
        if (!string.IsNullOrEmpty(request.Search))
        {
            var searchPattern = $"%{request.Search.Trim()}%";
            query = query.Where(u => (u.FirstName != null && EF.Functions.ILike(u.FirstName, searchPattern)) ||
                                     (u.LastName != null && EF.Functions.ILike(u.LastName, searchPattern)) ||
                                     (u.Email != null && EF.Functions.ILike(u.Email, searchPattern)) ||
                                     (u.Phone != null && EF.Functions.ILike(u.Phone, searchPattern)));
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
                ProfilePhotoUrl = u.ProfilePhotoUrl,
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

    public async Task<PagedResponse<ProfessionalProfileAdminDto>> GetProfessionalProfilesAsync(PagedRequest request, CancellationToken ct)
    {
        var query = context.ProfessionalProfiles
            .Include(p => p.User)
            .AsNoTracking();

        // Filter by Verification status if specified
        if (request.IsVerified.HasValue)
        {
            query = query.Where(p => p.IsVerified == request.IsVerified.Value);
        }

        // Filter by Search pattern (Name, Email, Phone, Headline)
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = $"%{request.Search.Trim()}%";
            query = query.Where(p =>
                (p.User.FirstName != null && EF.Functions.ILike(p.User.FirstName, pattern)) ||
                (p.User.LastName != null && EF.Functions.ILike(p.User.LastName, pattern)) ||
                (p.User.Email != null && EF.Functions.ILike(p.User.Email, pattern)) ||
                (p.User.Phone != null && EF.Functions.ILike(p.User.Phone, pattern)) ||
                (p.Headline != null && EF.Functions.ILike(p.Headline, pattern)));
        }

        // Filter by CreatedDate (if provided, e.g. "2026-09-01")
        if (!string.IsNullOrWhiteSpace(request.CreatedDate))
        {
            if (DateTime.TryParse(request.CreatedDate, out var parsedDate))
            {
                var startOfDay = DateTime.SpecifyKind(parsedDate.Date, DateTimeKind.Utc);
                var endOfDay = startOfDay.AddDays(1);
                query = query.Where(p => p.CreatedAt >= startOfDay && p.CreatedAt < endOfDay);
            }
        }

        var totalCount = await query.CountAsync(ct);

        // Sorting
        query = request.OrderBy switch
        {
            "Name" => request.Descending
                ? query.OrderByDescending(p => p.User.FirstName).ThenByDescending(p => p.User.LastName)
                : query.OrderBy(p => p.User.FirstName).ThenBy(p => p.User.LastName),
            "Email" => request.Descending
                ? query.OrderByDescending(p => p.User.Email)
                : query.OrderBy(p => p.User.Email),
            "YearsOfExperience" => request.Descending
                ? query.OrderByDescending(p => p.YearsOfExperience)
                : query.OrderBy(p => p.YearsOfExperience),
            "OverallRating" => request.Descending
                ? query.OrderByDescending(p => p.OverallRating)
                : query.OrderBy(p => p.OverallRating),
            "CompletedJobsCount" => request.Descending
                ? query.OrderByDescending(p => p.CompletedJobsCount)
                : query.OrderBy(p => p.CompletedJobsCount),
            "UpdatedAt" => request.Descending
                ? query.OrderByDescending(p => p.UpdatedAt)
                : query.OrderBy(p => p.UpdatedAt),
            _ => request.Descending
                ? query.OrderByDescending(p => p.CreatedAt)
                : query.OrderBy(p => p.CreatedAt)
        };

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProfessionalProfileAdminDto
            {
                Id = p.Id,
                UserId = p.UserId,
                FirstName = p.User.FirstName ?? "",
                LastName = p.User.LastName ?? "",
                Email = p.User.Email ?? "",
                Phone = p.User.Phone ?? "",
                ProfilePhotoUrl = p.User.ProfilePhotoUrl,
                Headline = p.Headline ?? "",
                Bio = p.Bio ?? "",
                ServiceRadiusKm = p.ServiceRadiusKm,
                YearsOfExperience = p.YearsOfExperience,
                OverallRating = p.OverallRating,
                CompletedJobsCount = p.CompletedJobsCount,
                IsVerified = p.IsVerified,
                UserStatus = p.User.Status,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync(ct);

        return new PagedResponse<ProfessionalProfileAdminDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<bool> ToggleVerifyProfessionalAsync(Guid userId, Guid adminUserId = default, CancellationToken ct = default)
    {
        var profile = await context.ProfessionalProfiles.FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (profile == null) return false;

        profile.IsVerified = !profile.IsVerified;
        profile.UpdatedAt = DateTime.UtcNow;

        var user = await context.Users.FindAsync(new object[] { userId }, ct);
        if (user != null && profile.IsVerified && user.Status == UserStatus.PendingVerification)
        {
            user.Status = UserStatus.Active;
            user.UpdatedAt = DateTime.UtcNow;
        }

        context.AdminLogs.Add(new AdminLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = adminUserId,
            Action = $"{(profile.IsVerified ? "Verified" : "Revoked verification for")} professional profile",
            TargetEntity = "ProfessionalProfile",
            TargetEntityId = user?.Id ?? profile.UserId,
            Timestamp = DateTime.UtcNow
        });

        await context.SaveChangesAsync(ct);
        return profile.IsVerified;
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

    public async Task<Guid> ApproveProfessionalAsync(Guid userId, Guid adminUserId = default, CancellationToken ct = default)
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
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Message = "Congratulations! Your professional profile has been approved and is now active.",
            Link = "/profile",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        context.Notifications.Add(notification);

        // Audit Log
        context.AdminLogs.Add(new AdminLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = adminUserId,
            Action = "Approved professional profile and activated account",
            TargetEntity = "ProfessionalProfile",
            TargetEntityId = user.Id,
            Timestamp = DateTime.UtcNow
        });

        await context.SaveChangesAsync(ct);

        // 📡 Broadcast live notification via SignalR
        var notifDto = new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Message = notification.Message,
            Link = notification.Link,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        };
        await hubContext.Clients.Group($"user_{user.Id}").ReceiveNotification(notifDto);

        return user.Id;
    }

    public async Task<Guid> SuspendProfessionalAsync(Guid userId, Guid adminUserId = default, CancellationToken ct = default)
    {
        var user = await context.Users.FindAsync(new object[] { userId }, ct);
        if (user == null) return Guid.Empty;

        user.Status = UserStatus.Suspended;
        user.UpdatedAt = DateTime.UtcNow;

        // Audit Log
        context.AdminLogs.Add(new AdminLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = adminUserId,
            Action = "Suspended user account for policy violation",
            TargetEntity = "User",
            TargetEntityId = user.Id,
            Timestamp = DateTime.UtcNow
        });

        await context.SaveChangesAsync(ct);
        return user.Id;
    }

    public async Task<Guid> ActivateUserAsync(Guid userId, Guid adminUserId = default, CancellationToken ct = default)
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

        // Notification of activation to user
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Message = "Your Callbet account has been activated by platform administration. Full access is restored.",
            Link = user.Role == UserRole.Professional ? "/professional-dashboard" : "/customer-dashboard",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        context.Notifications.Add(notification);

        // Audit Log
        context.AdminLogs.Add(new AdminLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = adminUserId,
            Action = "Restored and activated user account",
            TargetEntity = "User",
            TargetEntityId = user.Id,
            Timestamp = DateTime.UtcNow
        });

        await context.SaveChangesAsync(ct);

        // 📡 Broadcast live notification via SignalR
        var notifDto = new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Message = notification.Message,
            Link = notification.Link,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        };
        await hubContext.Clients.Group($"user_{user.Id}").ReceiveNotification(notifDto);

        return user.Id;
    }

    public async Task<Guid> DeleteUserAsync(Guid userId, Guid adminUserId = default, CancellationToken ct = default)
    {
        var user = await context.Users.FindAsync(new object[] { userId }, ct);
        if (user == null) return Guid.Empty;

        // Audit Log before deletion
        context.AdminLogs.Add(new AdminLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = adminUserId,
            Action = "Permanently deleted user account",
            TargetEntity = "User",
            TargetEntityId = user.Id,
            Timestamp = DateTime.UtcNow
        });

        context.Users.Remove(user);
        await context.SaveChangesAsync(ct);
        return user.Id;
    }

    public async Task<Guid> RegisterAdminAsync(UserDto dto, Guid adminUserId = default, CancellationToken ct = default)
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

        // Audit Log
        context.AdminLogs.Add(new AdminLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = adminUserId,
            Action = "Registered new system administrator staff",
            TargetEntity = "User",
            TargetEntityId = admin.Id,
            Timestamp = DateTime.UtcNow
        });

        await context.SaveChangesAsync(ct);
        return admin.Id;
    }

    // Service categories
    public async Task<Guid> CreateServiceCategoryAsync(ServiceCategoryDto dto, Guid adminUserId = default, CancellationToken ct = default)
    {
        var category = new ServiceCategory
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description
        };

        context.ServiceCategories.Add(category);

        // Audit Log
        context.AdminLogs.Add(new AdminLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = adminUserId,
            Action = $"Created service category: {category.Name}",
            TargetEntity = "ServiceCategory",
            TargetEntityId = category.Id,
            Timestamp = DateTime.UtcNow
        });

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
    public async Task<Guid> CreateServiceAsync(ServiceDto dto, Guid adminUserId = default, CancellationToken ct = default)
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

        // Audit Log
        context.AdminLogs.Add(new AdminLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = adminUserId,
            Action = $"Created catalog service: {service.Name}",
            TargetEntity = "Service",
            TargetEntityId = service.Id,
            Timestamp = DateTime.UtcNow
        });

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
    public async Task<Guid> AssignServiceAsync(ProfessionalServiceDto dto, Guid adminUserId = default, CancellationToken ct = default)
    {
        var existing = await context.ProfessionalServices
            .FirstOrDefaultAsync(ps => ps.ProfessionalProfileId == dto.ProfessionalProfileId && ps.ServiceId == dto.ServiceId, ct);

        if (existing != null)
        {
            existing.CustomPrice = dto.CustomPrice;
            existing.ExperienceYears = dto.ExperienceYears;
            await context.SaveChangesAsync(ct);
            return existing.Id;
        }

        var ps = new callbet.Domain.Entities.ProfessionalService
        {
            Id = Guid.NewGuid(),
            ProfessionalProfileId = dto.ProfessionalProfileId,
            ServiceId = dto.ServiceId,
            CustomPrice = dto.CustomPrice,
            ExperienceYears = dto.ExperienceYears
        };

        context.ProfessionalServices.Add(ps);

        // Audit Log
        context.AdminLogs.Add(new AdminLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = adminUserId,
            Action = "Assigned catalog service to professional profile",
            TargetEntity = "ProfessionalService",
            TargetEntityId = ps.Id,
            Timestamp = DateTime.UtcNow
        });

        await context.SaveChangesAsync(ct);
        return ps.Id;
    }

    public async Task<IEnumerable<object>> GetProfessionalServicesAsync(Guid profileId, CancellationToken ct)
    {
        return await context.ProfessionalServices
            .AsNoTracking()
            .Where(ps => ps.ProfessionalProfileId == profileId || ps.ProfessionalProfile.UserId == profileId)
            .Include(ps => ps.Service)
                .ThenInclude(s => s.Category)
            .Include(ps => ps.ProfessionalProfile)
                .ThenInclude(p => p.User)
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
    }

    public async Task<PagedResponse<AdminLogDto>> GetAdminLogsPagedAsync(PagedRequest request, CancellationToken ct)
    {
        var query = from l in context.AdminLogs.AsNoTracking()
                    join u in context.Users.AsNoTracking() on l.AdminUserId equals u.Id into uJoin
                    from admin in uJoin.DefaultIfEmpty()
                    join tu in context.Users.AsNoTracking() on l.TargetEntityId equals (Guid?)tu.Id into tuJoin
                    from targetUser in tuJoin.DefaultIfEmpty()
                    join pp in context.ProfessionalProfiles.AsNoTracking() on l.TargetEntityId equals (Guid?)pp.Id into ppJoin
                    from targetProf in ppJoin.DefaultIfEmpty()
                    join ppUser in context.Users.AsNoTracking() on targetProf.UserId equals ppUser.Id into ppUserJoin
                    from targetProfUser in ppUserJoin.DefaultIfEmpty()
                    select new AdminLogDto
                    {
                        Id = l.Id,
                        AdminUserId = l.AdminUserId,
                        AdminName = admin != null ? (admin.FirstName + " " + admin.LastName).Trim() : (l.AdminUserId == Guid.Empty ? "System Administrator" : "Admin"),
                        AdminEmail = admin != null ? admin.Email : (l.AdminUserId == Guid.Empty ? "system@callbet.et" : null),
                        Action = l.Action,
                        TargetEntity = l.TargetEntity,
                        TargetEntityId = l.TargetEntityId,
                        TargetUserName = targetUser != null
                            ? (targetUser.FirstName + " " + targetUser.LastName).Trim()
                            : (targetProfUser != null
                                ? (targetProfUser.FirstName + " " + targetProfUser.LastName).Trim()
                                : null),
                        TargetUserEmail = targetUser != null ? targetUser.Email : (targetProfUser != null ? targetProfUser.Email : null),
                        Timestamp = l.Timestamp
                    };

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = $"%{request.Search.Trim()}%";
            query = query.Where(l => EF.Functions.ILike(l.Action, search) ||
                                     (l.TargetEntity != null && EF.Functions.ILike(l.TargetEntity, search)) ||
                                     (l.AdminName != null && EF.Functions.ILike(l.AdminName, search)) ||
                                     (l.AdminEmail != null && EF.Functions.ILike(l.AdminEmail, search)) ||
                                     (l.TargetUserName != null && EF.Functions.ILike(l.TargetUserName, search)) ||
                                     (l.TargetUserEmail != null && EF.Functions.ILike(l.TargetUserEmail, search)));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(l => l.Timestamp)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return new PagedResponse<AdminLogDto>
        {
            Items = items,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<PagedResponse<FlaggedReviewAdminDto>> GetFlaggedReviewsAsync(PagedRequest request, CancellationToken ct = default)
    {
        var baseQuery = from r in context.Reviews.AsNoTracking()
                        join reviewer in context.Users.AsNoTracking() on r.ReviewerId equals reviewer.Id into rJoin
                        from reviewerUser in rJoin.DefaultIfEmpty()
                        join reviewee in context.Users.AsNoTracking() on r.RevieweeId equals reviewee.Id into reJoin
                        from revieweeUser in reJoin.DefaultIfEmpty()
                        where r.IsFlagged || r.Status != "Active"
                        select new { r, reviewerUser, revieweeUser };

        // 1. Filter by Status (e.g. "Flagged", "Dismissed", "Removed", "all")
        if (!string.IsNullOrWhiteSpace(request.Status) && !request.Status.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            var statusStr = request.Status.Trim();
            if (statusStr.Equals("Flagged", StringComparison.OrdinalIgnoreCase))
            {
                baseQuery = baseQuery.Where(x => x.r.IsFlagged && x.r.Status == "Active");
            }
            else
            {
                baseQuery = baseQuery.Where(x => EF.Functions.ILike(x.r.Status, statusStr));
            }
        }

        // 2. Filter by Search (Comment, FlagReason, Reviewer Name/Email, Professional Name/Email)
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchPattern = $"%{request.Search.Trim()}%";
            baseQuery = baseQuery.Where(x =>
                (x.r.Comment != null && EF.Functions.ILike(x.r.Comment, searchPattern)) ||
                (x.r.FlagReason != null && EF.Functions.ILike(x.r.FlagReason, searchPattern)) ||
                (x.reviewerUser != null && x.reviewerUser.FirstName != null && EF.Functions.ILike(x.reviewerUser.FirstName, searchPattern)) ||
                (x.reviewerUser != null && x.reviewerUser.LastName != null && EF.Functions.ILike(x.reviewerUser.LastName, searchPattern)) ||
                (x.reviewerUser != null && x.reviewerUser.Email != null && EF.Functions.ILike(x.reviewerUser.Email, searchPattern)) ||
                (x.revieweeUser != null && x.revieweeUser.FirstName != null && EF.Functions.ILike(x.revieweeUser.FirstName, searchPattern)) ||
                (x.revieweeUser != null && x.revieweeUser.LastName != null && EF.Functions.ILike(x.revieweeUser.LastName, searchPattern)) ||
                (x.revieweeUser != null && x.revieweeUser.Email != null && EF.Functions.ILike(x.revieweeUser.Email, searchPattern)));
        }

        var totalCount = await baseQuery.CountAsync(ct);

        // 3. Dynamic Ordering
        baseQuery = request.OrderBy switch
        {
            "Rating" => request.Descending ? baseQuery.OrderByDescending(x => x.r.Rating) : baseQuery.OrderBy(x => x.r.Rating),
            "Status" => request.Descending ? baseQuery.OrderByDescending(x => x.r.Status) : baseQuery.OrderBy(x => x.r.Status),
            _ => request.Descending ? baseQuery.OrderBy(x => x.r.CreatedAt) : baseQuery.OrderByDescending(x => x.r.CreatedAt)
        };

        // 4. Pagination and Projection
        var items = await baseQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new FlaggedReviewAdminDto
            {
                Id = x.r.Id,
                JobId = x.r.JobId,
                ReviewerId = x.r.ReviewerId,
                ReviewerName = x.reviewerUser != null ? (x.reviewerUser.FirstName + " " + x.reviewerUser.LastName).Trim() : "Anonymous Customer",
                RevieweeId = x.r.RevieweeId,
                ProfessionalName = x.revieweeUser != null ? (x.revieweeUser.FirstName + " " + x.revieweeUser.LastName).Trim() : "Assigned Professional",
                Rating = x.r.Rating,
                Comment = x.r.Comment,
                FlagReason = x.r.FlagReason ?? "Flagged by platform moderation",
                FlagDate = x.r.FlaggedAt ?? x.r.CreatedAt,
                Status = x.r.Status,
                CreatedAt = x.r.CreatedAt
            })
            .ToListAsync(ct);

        return new PagedResponse<FlaggedReviewAdminDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<bool> DismissReviewFlagAsync(Guid reviewId, Guid adminUserId = default, CancellationToken ct = default)
    {
        var review = await context.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId, ct);
        if (review == null) return false;

        review.IsFlagged = false;
        review.Status = "Dismissed";
        await context.SaveChangesAsync(ct);

        context.AdminLogs.Add(new AdminLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = adminUserId,
            Action = $"Dismissed moderation flag on customer review (Rating: {review.Rating}★)",
            TargetEntity = "Review",
            TargetEntityId = review.RevieweeId,
            Timestamp = DateTime.UtcNow
        });
        await context.SaveChangesAsync(ct);

        return true;
    }

    public async Task<IEnumerable<ServiceZoneDto>> GetServiceZonesAsync(CancellationToken ct = default)
    {
        var zones = await (from n in context.Neighborhoods.AsNoTracking()
                           join s in context.SubCities.AsNoTracking() on n.SubCityId equals s.Id
                           orderby s.Name, n.Name
                           select new ServiceZoneDto
                           {
                               Id = n.Id,
                               SubCityId = s.Id,
                               SubCity = s.Name,
                               Neighborhood = n.Name,
                               ActiveProsCount = context.Addresses
                                   .Where(a => a.NeighborhoodId == n.Id && a.User.Role == UserRole.Professional && a.User.Status == UserStatus.Active)
                                   .Select(a => a.UserId)
                                   .Distinct()
                                   .Count(),
                               Status = "Active",
                               CreatedAt = n.CreatedAt
                           }).ToListAsync(ct);

        return zones;
    }

    public async Task<ServiceZoneDto> CreateServiceZoneAsync(CreateServiceZoneDto dto, Guid adminUserId = default, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Neighborhood))
            throw new ArgumentException("Neighborhood name is required.");

        SubCity? subCity = null;
        if (dto.SubCityId.HasValue && dto.SubCityId.Value > 0)
        {
            subCity = await context.SubCities.FirstOrDefaultAsync(s => s.Id == dto.SubCityId.Value, ct);
        }
        else if (!string.IsNullOrWhiteSpace(dto.SubCity))
        {
            var subCityName = dto.SubCity.Trim();
            subCity = await context.SubCities.FirstOrDefaultAsync(s => EF.Functions.ILike(s.Name, subCityName), ct);
        }

        if (subCity == null)
        {
            var name = !string.IsNullOrWhiteSpace(dto.SubCity) ? dto.SubCity.Trim() : "Addis Ababa";
            subCity = new SubCity { Name = name, CreatedAt = DateTime.UtcNow };
            context.SubCities.Add(subCity);
            await context.SaveChangesAsync(ct);
        }

        var trimmedName = dto.Neighborhood.Trim();
        var existing = await context.Neighborhoods
            .FirstOrDefaultAsync(n => n.SubCityId == subCity.Id && EF.Functions.ILike(n.Name, trimmedName), ct);

        if (existing != null)
        {
            return new ServiceZoneDto
            {
                Id = existing.Id,
                SubCityId = subCity.Id,
                SubCity = subCity.Name,
                Neighborhood = existing.Name,
                ActiveProsCount = await context.Addresses
                    .Where(a => a.NeighborhoodId == existing.Id && a.User.Role == UserRole.Professional && a.User.Status == UserStatus.Active)
                    .Select(a => a.UserId)
                    .Distinct()
                    .CountAsync(ct),
                Status = "Active",
                CreatedAt = existing.CreatedAt
            };
        }

        var newNeighborhood = new Neighborhood
        {
            SubCityId = subCity.Id,
            Name = trimmedName,
            CreatedAt = DateTime.UtcNow
        };

        context.Neighborhoods.Add(newNeighborhood);
        await context.SaveChangesAsync(ct);

        context.AdminLogs.Add(new AdminLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = adminUserId,
            Action = $"Added new official service zone '{newNeighborhood.Name}' in Sub-City '{subCity.Name}'",
            TargetEntity = "ServiceZone",
            TargetEntityId = null,
            Timestamp = DateTime.UtcNow
        });
        await context.SaveChangesAsync(ct);

        return new ServiceZoneDto
        {
            Id = newNeighborhood.Id,
            SubCityId = subCity.Id,
            SubCity = subCity.Name,
            Neighborhood = newNeighborhood.Name,
            ActiveProsCount = 0,
            Status = "Active",
            CreatedAt = newNeighborhood.CreatedAt
        };
    }

    public async Task<bool> DeleteServiceZoneAsync(int neighborhoodId, Guid adminUserId = default, CancellationToken ct = default)
    {
        var neighborhood = await context.Neighborhoods
            .Include(n => n.SubCity)
            .FirstOrDefaultAsync(n => n.Id == neighborhoodId, ct);

        if (neighborhood == null) return false;

        var subCityName = neighborhood.SubCity?.Name ?? "Unknown";
        var neighborhoodName = neighborhood.Name;

        var linkedAddresses = await context.Addresses.Where(a => a.NeighborhoodId == neighborhoodId).ToListAsync(ct);
        foreach (var addr in linkedAddresses)
        {
            addr.NeighborhoodId = null;
        }

        context.Neighborhoods.Remove(neighborhood);
        await context.SaveChangesAsync(ct);

        context.AdminLogs.Add(new AdminLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = adminUserId,
            Action = $"Deleted official service zone '{neighborhoodName}' from Sub-City '{subCityName}'",
            TargetEntity = "ServiceZone",
            TargetEntityId = null,
            Timestamp = DateTime.UtcNow
        });
        await context.SaveChangesAsync(ct);

        return true;
    }

    public async Task<PagedResponse<AdminReportDto>> GetReportsAsync(PagedRequest request, CancellationToken ct = default)
    {
        var baseQuery = from r in context.Reports.AsNoTracking()
                        join reporter in context.Users.AsNoTracking() on r.ReporterUserId equals reporter.Id into repJoin
                        from rep in repJoin.DefaultIfEmpty()
                        join reported in context.Users.AsNoTracking() on r.ReportedUserId equals (Guid?)reported.Id into repedJoin
                        from reped in repedJoin.DefaultIfEmpty()
                        select new { r, rep, reped };

        // 1. Filter by Resolution Status (e.g. "pending", "resolved", "all", or IsVerified bool)
        if (!string.IsNullOrWhiteSpace(request.Status) && !request.Status.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            var statusStr = request.Status.Trim();
            if (statusStr.Equals("resolved", StringComparison.OrdinalIgnoreCase) ||
                statusStr.Equals("closed", StringComparison.OrdinalIgnoreCase) ||
                statusStr.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                baseQuery = baseQuery.Where(x => x.r.IsResolved == true);
            }
            else if (statusStr.Equals("pending", StringComparison.OrdinalIgnoreCase) ||
                     statusStr.Equals("open", StringComparison.OrdinalIgnoreCase) ||
                     statusStr.Equals("unresolved", StringComparison.OrdinalIgnoreCase) ||
                     statusStr.Equals("false", StringComparison.OrdinalIgnoreCase))
            {
                baseQuery = baseQuery.Where(x => x.r.IsResolved == false);
            }
        }
        else if (request.IsVerified.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.r.IsResolved == request.IsVerified.Value);
        }

        // 2. Filter by Role (Reporter user role e.g. Customer, Professional)
        if (!string.IsNullOrWhiteSpace(request.Role) && !request.Role.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            if (Enum.TryParse<UserRole>(request.Role.Trim(), true, out var parsedRole))
            {
                baseQuery = baseQuery.Where(x => x.rep != null && x.rep.Role == parsedRole);
            }
        }

        // 3. Filter by Search pattern (Reason, Details, Reporter Name/Email, Reported Name/Email)
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchPattern = $"%{request.Search.Trim()}%";
            baseQuery = baseQuery.Where(x =>
                EF.Functions.ILike(x.r.Reason, searchPattern) ||
                (x.r.Details != null && EF.Functions.ILike(x.r.Details, searchPattern)) ||
                (x.rep != null && x.rep.FirstName != null && EF.Functions.ILike(x.rep.FirstName, searchPattern)) ||
                (x.rep != null && x.rep.LastName != null && EF.Functions.ILike(x.rep.LastName, searchPattern)) ||
                (x.rep != null && x.rep.Email != null && EF.Functions.ILike(x.rep.Email, searchPattern)) ||
                (x.reped != null && x.reped.FirstName != null && EF.Functions.ILike(x.reped.FirstName, searchPattern)) ||
                (x.reped != null && x.reped.LastName != null && EF.Functions.ILike(x.reped.LastName, searchPattern)) ||
                (x.reped != null && x.reped.Email != null && EF.Functions.ILike(x.reped.Email, searchPattern)));
        }

        var totalCount = await baseQuery.CountAsync(ct);

        // 4. Dynamic Sorting
        baseQuery = request.OrderBy switch
        {
            "Reason" => request.Descending ? baseQuery.OrderByDescending(x => x.r.Reason) : baseQuery.OrderBy(x => x.r.Reason),
            "Status" => request.Descending ? baseQuery.OrderByDescending(x => x.r.IsResolved) : baseQuery.OrderBy(x => x.r.IsResolved),
            _ => request.Descending ? baseQuery.OrderBy(x => x.r.CreatedAt) : baseQuery.OrderByDescending(x => x.r.CreatedAt)
        };

        // 5. Pagination and Projection to DTO
        var items = await baseQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new AdminReportDto
            {
                Id = x.r.Id,
                ReporterUserId = x.r.ReporterUserId,
                ReporterName = x.rep != null ? (x.rep.FirstName + " " + x.rep.LastName).Trim() : "Anonymous User",
                ReporterEmail = x.rep != null ? (x.rep.Email ?? "Unknown") : "Unknown",
                ReporterRole = x.rep != null ? x.rep.Role.ToString() : "Customer",
                ReportedUserId = x.r.ReportedUserId,
                ReportedName = (x.r.ReportedUserId == null || x.r.ReportedUserId == Guid.Empty)
                    ? "Platform / General Service"
                    : (x.reped != null ? (x.reped.FirstName + " " + x.reped.LastName).Trim() : "Reported User"),
                ReportedEmail = x.reped != null ? x.reped.Email : null,
                ReportedRole = x.reped != null ? x.reped.Role.ToString() : null,
                Reason = x.r.Reason,
                Details = x.r.Details,
                CreatedAt = x.r.CreatedAt,
                IsResolved = x.r.IsResolved
            })
            .ToListAsync(ct);

        return new PagedResponse<AdminReportDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<bool> ResolveReportAsync(Guid reportId, Guid adminUserId = default, CancellationToken ct = default)
    {
        var report = await context.Reports.FirstOrDefaultAsync(r => r.Id == reportId, ct);
        if (report == null) return false;

        report.IsResolved = true;

        // 1. Notify the Reporter (Customer or Professional)
        var reporter = await context.Users.FirstOrDefaultAsync(u => u.Id == report.ReporterUserId, ct);
        if (reporter != null)
        {
            var targetLink = reporter.Role == UserRole.Professional ? "/professional-notifications" : "/customer-notifications";
            var reporterNotification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = reporter.Id,
                Message = $"Your incident report regarding '{report.Reason}' has been reviewed and marked as Resolved by Administration.",
                Link = targetLink,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            context.Notifications.Add(reporterNotification);

            var notifDto = new NotificationDto
            {
                Id = reporterNotification.Id,
                UserId = reporterNotification.UserId,
                Message = reporterNotification.Message,
                Link = reporterNotification.Link,
                IsRead = false,
                CreatedAt = reporterNotification.CreatedAt
            };
            await hubContext.Clients.Group($"user_{reporter.Id}").ReceiveNotification(notifDto);
        }

        // 2. Also notify the Reported User if specified
        if (report.ReportedUserId.HasValue && report.ReportedUserId.Value != Guid.Empty && report.ReportedUserId.Value != report.ReporterUserId)
        {
            var reportedUser = await context.Users.FirstOrDefaultAsync(u => u.Id == report.ReportedUserId.Value, ct);
            if (reportedUser != null)
            {
                var targetLink = reportedUser.Role == UserRole.Professional ? "/professional-notifications" : "/customer-notifications";
                var reportedNotification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = reportedUser.Id,
                    Message = $"The reported inquiry concerning '{report.Reason}' has been officially reviewed and closed by Administration.",
                    Link = targetLink,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                context.Notifications.Add(reportedNotification);

                var reportedNotifDto = new NotificationDto
                {
                    Id = reportedNotification.Id,
                    UserId = reportedNotification.UserId,
                    Message = reportedNotification.Message,
                    Link = reportedNotification.Link,
                    IsRead = false,
                    CreatedAt = reportedNotification.CreatedAt
                };
                await hubContext.Clients.Group($"user_{reportedUser.Id}").ReceiveNotification(reportedNotifDto);
            }
        }

        // 3. Record Admin Audit Log
        context.AdminLogs.Add(new AdminLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = adminUserId,
            Action = $"Marked user incident report '{report.Reason}' as Resolved and notified user",
            TargetEntity = "Report",
            TargetEntityId = report.ReportedUserId,
            Timestamp = DateTime.UtcNow
        });
        await context.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> DeleteReportAsync(Guid reportId, Guid adminUserId = default, CancellationToken ct = default)
    {
        var report = await context.Reports.FirstOrDefaultAsync(r => r.Id == reportId, ct);
        if (report == null) return false;

        context.Reports.Remove(report);
        await context.SaveChangesAsync(ct);

        context.AdminLogs.Add(new AdminLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = adminUserId,
            Action = $"Deleted incident report '{report.Reason}' from system records",
            TargetEntity = "Report",
            TargetEntityId = report.ReportedUserId,
            Timestamp = DateTime.UtcNow
        });
        await context.SaveChangesAsync(ct);

        return true;
    }
}

