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

        if (existing != null)
        {
            existing.Headline = dto.Headline;
            existing.Bio = dto.Bio;
            existing.ServiceRadiusKm = dto.ServiceRadiusKm;
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
            ServiceRadiusKm = dto.ServiceRadiusKm,
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
        var certificate = new Certificate
        {
            Id = Guid.NewGuid(),
            ProfessionalProfileId = dto.ProfessionalProfileId,
            Title = dto.Title,
            Organization = dto.Organization,
            IssueDate = dto.IssueDate,
            ExpiryDate = dto.ExpiryDate,
            DocumentImageUrl = dto.DocumentImageUrl
        };

        context.Certificates.Add(certificate);
        await context.SaveChangesAsync(ct);
        return certificate.Id;
    }

    public async Task<Guid> AddPortfolioItemAsync(PortfolioItemDto dto, CancellationToken ct)
    {
        var item = new PortfolioItem
        {
            Id = Guid.NewGuid(),
            ProfessionalProfileId = dto.ProfessionalProfileId,
            Title = dto.Title,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl,
            DateCompleted = dto.DateCompleted
        };

        context.PortfolioItems.Add(item);
        await context.SaveChangesAsync(ct);
        return item.Id;
    }

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
            .Include(p => p.Services)
                .ThenInclude(ps => ps.Service)
                    .ThenInclude(s => s.Category)
            .FirstOrDefaultAsync(p => p.Id == id || p.UserId == id || p.Services.Any(ps => ps.Id == id), ct);

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

        return new ProfessionalProfileDetailsDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            FullName = profile.User.FirstName + " " + profile.User.LastName,
            Email = profile.User.Email,
            Phone = profile.User.Phone,
            ProfilePhotoUrl = profile.User.ProfilePhotoUrl,
            Headline = profile.Headline,
            Bio = profile.Bio,
            ServiceRadiusKm = profile.ServiceRadiusKm,
            YearsOfExperience = profile.YearsOfExperience,
            OverallRating = profile.OverallRating,
            CompletedJobsCount = profile.CompletedJobsCount,
            IsVerified = profile.IsVerified,
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
            Certificates = profile.Certificates.Select(c => new CertificateDto
            {
                Id = c.Id,
                ProfessionalProfileId = c.ProfessionalProfileId,
                Title = c.Title,
                Organization = c.Organization,
                IssueDate = c.IssueDate,
                ExpiryDate = c.ExpiryDate,
                DocumentImageUrl = c.DocumentImageUrl
            }).ToList(),
            PortfolioItems = portfolioItems,
            Services = profile.Services.Select(ps => new ProfessionalProfileServiceDto
            {
                Id = ps.Id,
                ProfessionalProfileId = ps.ProfessionalProfileId,
                UserId = profile.UserId,
                ProfessionalName = profile.User.FirstName + " " + profile.User.LastName,
                ProfessionalHeadline = profile.Headline,
                ProfilePhotoUrl = profile.User.ProfilePhotoUrl,
                OverallRating = profile.OverallRating,
                CompletedJobsCount = profile.CompletedJobsCount,
                IsVerified = profile.IsVerified,
                ProfessionalExperienceYears = profile.YearsOfExperience,
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
            }).ToList(),
            AvailabilitySchedules = profile.AvailabilitySchedules.Select(s => new AvailabilityScheduleDto
            {
                Id = s.Id,
                ProfessionalProfileId = s.ProfessionalProfileId,
                DayOfWeek = s.DayOfWeek,
                StartTime = s.StartTime,
                EndTime = s.EndTime
            }).OrderBy(s => s.DayOfWeek).ThenBy(s => s.StartTime).ToList(),
            Reviews = reviews
        };
    }
}
