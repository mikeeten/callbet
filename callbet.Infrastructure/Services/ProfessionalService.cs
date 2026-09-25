using callbet.Application.DTOs;
using callbet.Application.Interfaces;
using callbet.Domain.Entities;
using callbet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace callbet.Infrastructure.Services;

public class ProfessionalService(CallbetDbContext context) : IProfessionalService
{
    // public async Task<Guid> RegisterProfessionalAsync(UserDto dto, CancellationToken ct)
    // {
    //     var user = new User
    //     {
    //         Id = Guid.NewGuid(),
    //         FirstName = dto.FirstName,
    //         LastName = dto.LastName,
    //         Email = dto.Email,
    //         Phone = dto.Phone,
    //         PasswordHash = dto.PasswordHash,
    //         Role = UserRole.Professional,
    //         Status = UserStatus.PendingVerification,
    //         CreatedAt = DateTime.UtcNow,
    //         UpdatedAt = DateTime.UtcNow
    //     };

    //     context.Users.Add(user);
    //     await context.SaveChangesAsync(ct);
    //     return user.Id;
    // }

    public async Task<Guid> CreateProfileAsync(ProfessionalProfileDto dto, CancellationToken ct)
    {
        var existing = await context.ProfessionalProfiles
            .FirstOrDefaultAsync(p => p.UserId == dto.UserId, ct);

        int radius = (int)Math.Max(1, Math.Round(dto.ServiceRadiusKm > 0 ? dto.ServiceRadiusKm : 15));

        if (existing != null)
        {
            existing.Headline = dto.Headline;
            existing.Bio = dto.Bio;
            existing.ServiceRadiusKm = radius;
            existing.YearsOfExperience = dto.YearsOfExperience;
            existing.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(ct);
            return existing.Id;
        }

        var profile = new ProfessionalProfile
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            Headline = dto.Headline,
            Bio = dto.Bio,
            ServiceRadiusKm = radius,
            YearsOfExperience = dto.YearsOfExperience,
            OverallRating = dto.OverallRating,
            CompletedJobsCount = dto.CompletedJobsCount,
            IsVerified = dto.IsVerified,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.ProfessionalProfiles.Add(profile);
        await context.SaveChangesAsync(ct);
        return profile.Id;
    }

    public async Task<Guid> UploadResumeAsync(ResumeDto dto, CancellationToken ct)
    {
        var existing = await context.Resumes
            .FirstOrDefaultAsync(r => r.ProfessionalProfileId == dto.ProfessionalProfileId, ct);

        if (existing != null)
        {
            existing.Summary = dto.Summary;
            existing.EducationJson = dto.EducationJson;
            existing.ExperienceJson = dto.ExperienceJson;
            existing.Skills = dto.Skills ?? new List<string>();
            existing.Languages = dto.Languages;
            existing.ResumeFileUrl = dto.ResumeFileUrl;
            existing.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(ct);
            return existing.Id;
        }

        var resume = new Resume
        {
            Id = Guid.NewGuid(),
            ProfessionalProfileId = dto.ProfessionalProfileId,
            Summary = dto.Summary,
            EducationJson = dto.EducationJson,
            ExperienceJson = dto.ExperienceJson,
            Skills = dto.Skills ?? new List<string>(),
            Languages = dto.Languages,
            ResumeFileUrl = dto.ResumeFileUrl,
            UpdatedAt = DateTime.UtcNow
        };

        context.Resumes.Add(resume);
        await context.SaveChangesAsync(ct);
        return resume.Id;
    }

    public async Task<Guid> AddCertificateAsync(CertificateDto dto, CancellationToken ct)
    {
        var issueDate = dto.IssueDate.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(dto.IssueDate, DateTimeKind.Utc)
            : dto.IssueDate.ToUniversalTime();

        DateTime? expiryDate = dto.ExpiryDate.HasValue
            ? (dto.ExpiryDate.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(dto.ExpiryDate.Value, DateTimeKind.Utc)
                : dto.ExpiryDate.Value.ToUniversalTime())
            : null;

        var certificate = new Certificate
        {
            Id = Guid.NewGuid(),
            ProfessionalProfileId = dto.ProfessionalProfileId,
            Title = dto.Title,
            Organization = dto.Organization,
            IssueDate = issueDate,
            ExpiryDate = expiryDate,
            DocumentImageUrl = dto.DocumentImageUrl
        };

        context.Certificates.Add(certificate);
        await context.SaveChangesAsync(ct);
        return certificate.Id;
    }

    public async Task<Guid> AddPortfolioItemAsync(PortfolioItemDto dto, CancellationToken ct)
    {
        var dateCompleted = dto.DateCompleted.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(dto.DateCompleted, DateTimeKind.Utc)
            : dto.DateCompleted.ToUniversalTime();

        var item = new PortfolioItem
        {
            Id = Guid.NewGuid(),
            ProfessionalProfileId = dto.ProfessionalProfileId,
            Title = dto.Title,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl,
            DateCompleted = dateCompleted
        };

        context.PortfolioItems.Add(item);
        await context.SaveChangesAsync(ct);
        return item.Id;
    }

    public async Task<Guid> AssignServiceAsync(ProfessionalServiceDto dto, CancellationToken ct)
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
        await context.SaveChangesAsync(ct);
        return ps.Id;
    }

    public async Task<bool> UnassignServiceAsync(Guid professionalProfileId, Guid serviceId, CancellationToken ct = default)
    {
        var item = await context.ProfessionalServices
            .FirstOrDefaultAsync(ps => (ps.ProfessionalProfileId == professionalProfileId || ps.ProfessionalProfile.UserId == professionalProfileId)
                                    && (ps.ServiceId == serviceId || ps.Id == serviceId), ct);
        if (item == null) return false;

        context.ProfessionalServices.Remove(item);
        await context.SaveChangesAsync(ct);
        return true;
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

    public async Task<bool> RefreshReputationAsync(Guid userId, CancellationToken ct)
    {
        var profile = await context.ProfessionalProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId || p.Id == userId, ct);

        if (profile == null) return false;

        // SELECT AVG(rating) FROM reviews WHERE professional_id = userId
        var avgRating = await context.Reviews
            .Where(r => r.RevieweeId == profile.UserId)
            .AverageAsync(r => (decimal?)r.Rating, ct) ?? 0.00m;

        // SELECT COUNT(*) FROM jobs WHERE professional_id = userId AND status = 'completed'
        var completedJobs = await context.Jobs
            .CountAsync(j => j.ProfessionalId == profile.UserId &&
                            (j.Status == JobStatus.CompletedPendingApproval || j.Status == JobStatus.Closed), ct);

        profile.OverallRating = Math.Round(avgRating, 2);
        profile.CompletedJobsCount = completedJobs;
        profile.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<Guid> AddAvailabilityScheduleAsync(AvailabilityScheduleDto dto, CancellationToken ct)
    {
        var profile = await context.ProfessionalProfiles
            .FirstOrDefaultAsync(p => p.Id == dto.ProfessionalProfileId || p.UserId == dto.ProfessionalProfileId, ct);

        var profileId = profile != null ? profile.Id : dto.ProfessionalProfileId;

        var schedule = new AvailabilitySchedule
        {
            Id = dto.Id != Guid.Empty ? dto.Id : Guid.NewGuid(),
            ProfessionalProfileId = profileId,
            DayOfWeek = dto.DayOfWeek,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime
        };

        context.AvailabilitySchedules.Add(schedule);
        await context.SaveChangesAsync(ct);
        return schedule.Id;
    }

    public async Task<IEnumerable<AvailabilityScheduleDto>> GetAvailabilitySchedulesAsync(Guid profileId, CancellationToken ct)
    {
        var profile = await context.ProfessionalProfiles
            .FirstOrDefaultAsync(p => p.Id == profileId || p.UserId == profileId, ct);

        var targetId = profile != null ? profile.Id : profileId;

        return await context.AvailabilitySchedules
            .Where(s => s.ProfessionalProfileId == targetId)
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .Select(s => new AvailabilityScheduleDto
            {
                Id = s.Id,
                ProfessionalProfileId = s.ProfessionalProfileId,
                DayOfWeek = s.DayOfWeek,
                StartTime = s.StartTime,
                EndTime = s.EndTime
            })
            .ToListAsync(ct);
    }

    public async Task<ProfessionalProfileDetailsDto?> GetProfileDetailsAsync(Guid id, CancellationToken ct)
    {
        var profile = await context.ProfessionalProfiles
            .Include(p => p.User)
            .Include(p => p.Resume)
            .Include(p => p.Certificates)
            .Include(p => p.AvailabilitySchedules)
            .FirstOrDefaultAsync(p => p.Id == id || p.UserId == id, ct);

        if (profile == null) return null;

        var portfolioItems = await context.PortfolioItems
            .Where(pi => pi.ProfessionalProfileId == profile.Id)
            .Select(pi => new PortfolioItemDto
            {
                Id = pi.Id,
                ProfessionalProfileId = pi.ProfessionalProfileId,
                Title = pi.Title,
                Description = pi.Description,
                ImageUrl = pi.ImageUrl,
                DateCompleted = pi.DateCompleted
            })
            .ToListAsync(ct);

        var reviews = await context.Reviews
            .Where(r => r.RevieweeId == profile.UserId)
            .Join(context.Users,
                  r => r.ReviewerId,
                  u => u.Id,
                  (r, u) => new ReviewDto
                  {
                      Id = r.Id,
                      JobId = r.JobId,
                      ReviewerId = r.ReviewerId,
                      RevieweeId = r.RevieweeId,
                      Rating = r.Rating,
                      Comment = r.Comment,
                      CustomerName = u.FirstName + " " + u.LastName,
                      CreatedAt = r.CreatedAt,
                      Reply = r.Reply == null ? null : new ReviewReplyDto
                      {
                          Id = r.Reply.Id,
                          ReviewId = r.Reply.ReviewId,
                          ReplierId = r.Reply.ReplierId,
                          Comment = r.Reply.Comment,
                          CreatedAt = r.Reply.CreatedAt
                      }
                  })
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

        var addresses = await context.Addresses
            .Include(a => a.Neighborhood)
                .ThenInclude(n => n!.SubCity)
            .Where(a => a.UserId == profile.UserId)
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

        var baseAddress = addresses.FirstOrDefault();

        return new ProfessionalProfileDetailsDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            FullName = profile.User != null ? (profile.User.FirstName + " " + profile.User.LastName).Trim() : "Professional",
            Email = profile.User?.Email ?? string.Empty,
            Phone = profile.User?.Phone ?? string.Empty,
            ProfilePhotoUrl = profile.User?.ProfilePhotoUrl,
            Headline = profile.Headline,
            Bio = profile.Bio,
            ServiceRadiusKm = profile.ServiceRadiusKm,
            YearsOfExperience = profile.YearsOfExperience,
            OverallRating = profile.OverallRating,
            CompletedJobsCount = profile.CompletedJobsCount,
            IsVerified = profile.IsVerified,
            BaseAddress = baseAddress,
            Addresses = addresses,
            Resume = profile.Resume == null ? null : new ResumeDto
            {
                Id = profile.Resume.Id,
                ProfessionalProfileId = profile.Resume.ProfessionalProfileId,
                Summary = profile.Resume.Summary,
                EducationJson = profile.Resume.EducationJson,
                ExperienceJson = profile.Resume.ExperienceJson,
                Skills = profile.Resume.Skills,
                Languages = profile.Resume.Languages,
                ResumeFileUrl = profile.Resume.ResumeFileUrl
            },
            Certificates = profile.Certificates != null ? profile.Certificates.Select(c => new CertificateDto
            {
                Id = c.Id,
                ProfessionalProfileId = c.ProfessionalProfileId,
                Title = c.Title,
                Organization = c.Organization,
                IssueDate = c.IssueDate,
                ExpiryDate = c.ExpiryDate,
                DocumentImageUrl = c.DocumentImageUrl
            }).ToList() : new List<CertificateDto>(),
            PortfolioItems = portfolioItems ?? new List<PortfolioItemDto>(),
            AvailabilitySchedules = profile.AvailabilitySchedules != null ? profile.AvailabilitySchedules.Select(s => new AvailabilityScheduleDto
            {
                Id = s.Id,
                ProfessionalProfileId = s.ProfessionalProfileId,
                DayOfWeek = s.DayOfWeek,
                StartTime = s.StartTime,
                EndTime = s.EndTime
            }).OrderBy(s => s.DayOfWeek).ThenBy(s => s.StartTime).ToList() : new List<AvailabilityScheduleDto>(),
            Reviews = reviews ?? new List<ReviewDto>()
        };
    }

    public async Task<GetProfessionalProfileDashboardDto?> GetProfessionalProfileDashboardAsync(Guid id, CancellationToken ct)
    {
        var profile = await context.ProfessionalProfiles
            .Include(p => p.User)
            .Include(p => p.Resume)
            .Include(p => p.Certificates)
            .Include(p => p.AvailabilitySchedules)
            .FirstOrDefaultAsync(p => p.Id == id || p.UserId == id, ct);

        if (profile == null) return null;

        var portfolioItems = await context.PortfolioItems
            .Where(pi => pi.ProfessionalProfileId == profile.Id)
            .Select(pi => new PortfolioItemDto
            {
                Id = pi.Id,
                ProfessionalProfileId = pi.ProfessionalProfileId,
                Title = pi.Title,
                Description = pi.Description,
                ImageUrl = pi.ImageUrl,
                DateCompleted = pi.DateCompleted
            })
            .ToListAsync(ct);

        var reviews = await context.Reviews
            .Where(r => r.RevieweeId == profile.UserId)
            .Join(context.Users,
                  r => r.ReviewerId,
                  u => u.Id,
                  (r, u) => new ReviewDto
                  {
                      Id = r.Id,
                      JobId = r.JobId,
                      ReviewerId = r.ReviewerId,
                      RevieweeId = r.RevieweeId,
                      Rating = r.Rating,
                      Comment = r.Comment,
                      CustomerName = u.FirstName + " " + u.LastName,
                      CreatedAt = r.CreatedAt,
                      Reply = r.Reply == null ? null : new ReviewReplyDto
                      {
                          Id = r.Reply.Id,
                          ReviewId = r.Reply.ReviewId,
                          ReplierId = r.Reply.ReplierId,
                          Comment = r.Reply.Comment,
                          CreatedAt = r.Reply.CreatedAt
                      }
                  })
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

        var addresses = await context.Addresses
            .Include(a => a.Neighborhood)
                .ThenInclude(n => n!.SubCity)
            .Where(a => a.UserId == profile.UserId)
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

        var baseAddress = addresses.FirstOrDefault();

        return new GetProfessionalProfileDashboardDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            FullName = profile.User != null ? (profile.User.FirstName + " " + profile.User.LastName).Trim() : "Professional",
            Email = profile.User?.Email ?? string.Empty,
            Phone = profile.User?.Phone ?? string.Empty,
            ProfilePhotoUrl = profile.User?.ProfilePhotoUrl,
            Headline = profile.Headline,
            Bio = profile.Bio,
            ServiceRadiusKm = profile.ServiceRadiusKm,
            YearsOfExperience = profile.YearsOfExperience,
            OverallRating = profile.OverallRating,
            CompletedJobsCount = profile.CompletedJobsCount,
            IsVerified = profile.IsVerified,
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt,
            BaseAddress = baseAddress,
            Addresses = addresses,
            Resume = profile.Resume == null ? null : new ResumeDto
            {
                Id = profile.Resume.Id,
                ProfessionalProfileId = profile.Resume.ProfessionalProfileId,
                Summary = profile.Resume.Summary,
                EducationJson = profile.Resume.EducationJson,
                ExperienceJson = profile.Resume.ExperienceJson,
                Skills = profile.Resume.Skills,
                Languages = profile.Resume.Languages,
                ResumeFileUrl = profile.Resume.ResumeFileUrl
            },
            Certificates = profile.Certificates != null ? profile.Certificates.Select(c => new CertificateDto
            {
                Id = c.Id,
                ProfessionalProfileId = c.ProfessionalProfileId,
                Title = c.Title,
                Organization = c.Organization,
                IssueDate = c.IssueDate,
                ExpiryDate = c.ExpiryDate,
                DocumentImageUrl = c.DocumentImageUrl
            }).ToList() : new List<CertificateDto>(),
            PortfolioItems = portfolioItems ?? new List<PortfolioItemDto>(),
            AvailabilitySchedules = profile.AvailabilitySchedules != null ? profile.AvailabilitySchedules.Select(s => new AvailabilityScheduleDto
            {
                Id = s.Id,
                ProfessionalProfileId = s.ProfessionalProfileId,
                DayOfWeek = s.DayOfWeek,
                StartTime = s.StartTime,
                EndTime = s.EndTime
            }).OrderBy(s => s.DayOfWeek).ThenBy(s => s.StartTime).ToList() : new List<AvailabilityScheduleDto>(),
            Reviews = reviews ?? new List<ReviewDto>()
        };
    }

    public async Task<bool> DeleteAvailabilityScheduleAsync(Guid professionalProfileId, Guid scheduleId, CancellationToken ct = default)
    {
        var item = await context.AvailabilitySchedules
            .FirstOrDefaultAsync(s => s.Id == scheduleId && (s.ProfessionalProfileId == professionalProfileId || s.ProfessionalProfile.UserId == professionalProfileId), ct);
        if (item == null) return false;

        context.AvailabilitySchedules.Remove(item);
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteCertificateAsync(Guid professionalProfileId, Guid certificateId, CancellationToken ct)
    {
        var item = await context.Certificates
            .FirstOrDefaultAsync(c => c.Id == certificateId && (c.ProfessionalProfileId == professionalProfileId || c.ProfessionalProfile.UserId == professionalProfileId), ct);
        if (item == null) return false;

        context.Certificates.Remove(item);
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeletePortfolioItemAsync(Guid professionalProfileId, Guid portfolioItemId, CancellationToken ct)
    {
        var item = await context.PortfolioItems
            .FirstOrDefaultAsync(pi => pi.Id == portfolioItemId && pi.ProfessionalProfileId == professionalProfileId, ct);
        if (item == null) return false;

        context.PortfolioItems.Remove(item);
        await context.SaveChangesAsync(ct);
        return true;
    }
}
