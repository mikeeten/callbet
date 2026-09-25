using callbet.Application.DTOs;
using callbet.Application.Interfaces;
using callbet.Domain.Entities;
using callbet.Infrastructure.Hubs;
using callbet.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace callbet.Infrastructure.Services;

public class VerificationService(
    CallbetDbContext context,
    IHubContext<ChatHub, IChatHubClient> hubContext) : IVerificationService
{
    public async Task<Guid> UploadDocumentAsync(VerificationRecordDto dto, CancellationToken ct)
    {
        var record = new VerificationRecord
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            DocumentType = dto.DocumentType,
            DocumentUrl = dto.DocumentUrl,
            IsVerified = false,
            // CreatedAt = DateTime.UtcNow
        };

        context.VerificationRecords.Add(record);

        // 📝 Log pending verification for admin review
        context.AdminLogs.Add(new AdminLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = Guid.Empty,
            Action = $"Verification pending for Professional {dto.UserId}",
            TargetEntity = "VerificationRecord",
            TargetEntityId = record.Id,
            Timestamp = DateTime.UtcNow
        });

        await context.SaveChangesAsync(ct);
        return record.Id;
    }

    public async Task<PagedResponse<VerificationRecord>> GetVerificationRecordsAsync(
        PagedRequest request, CancellationToken ct)
    {
        var query = context.VerificationRecords.AsNoTracking();

        // Filter by verification status
        if (request.IsVerified.HasValue)
        {
            query = query.Where(vr => vr.IsVerified == request.IsVerified.Value);
        }

        // Search by document type or user
        if (!string.IsNullOrEmpty(request.Search))
        {
            var searchPattern = $"%{request.Search}%";
            query = query.Where(vr => EF.Functions.ILike(vr.DocumentType, searchPattern)); 
            // ||
                                    //  EF.Functions.ILike(vr.DocumentUrl, searchPattern));
        }

        // Count before paging
        var totalCount = await query.CountAsync(ct);

        // Order by safe columns
        query = request.OrderBy switch
        {
            "DocumentType" => request.Descending ? query.OrderByDescending(vr => vr.DocumentType) : query.OrderBy(vr => vr.DocumentType),
            "VerificationDate" => request.Descending ? query.OrderByDescending(vr => vr.VerificationDate) : query.OrderBy(vr => vr.VerificationDate),
            _ => query.OrderBy(vr => vr.VerificationDate)
        };

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return new PagedResponse<VerificationRecord>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<Guid> ApproveVerificationAsync(Guid recordId, Guid adminUserId = default, CancellationToken ct = default)
    {
        var record = await context.VerificationRecords.FindAsync(new object[] { recordId }, ct);
        if (record == null) return Guid.Empty;

        record.IsVerified = true;
        record.VerificationDate = DateTime.UtcNow;

        var user = await context.Users.FindAsync(new object[] { record.UserId }, ct);
        if (user != null && user.Status == UserStatus.PendingVerification)
        {
            user.Status = UserStatus.Active;
            user.UpdatedAt = DateTime.UtcNow;
        }

        var profile = await context.ProfessionalProfiles.FirstOrDefaultAsync(p => p.UserId == record.UserId, ct);
        if (profile != null)
        {
            profile.IsVerified = true;
            profile.UpdatedAt = DateTime.UtcNow;
        }

        // Notification of approval to professional
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = record.UserId,
            Message = "Your verification document has been approved! Your account is now verified.",
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
            Action = $"Approved professional verification ({record.DocumentType})",
            TargetEntity = "VerificationRecord",
            TargetEntityId = record.Id,
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
        await hubContext.Clients.Group($"user_{record.UserId}").ReceiveNotification(notifDto);

        return record.Id;
    }

    public async Task<Guid> DeleteVerificationAsync(Guid recordId, Guid adminUserId = default, CancellationToken ct = default)
    {
        var record = await context.VerificationRecords.FindAsync(new object[] { recordId }, ct);
        if (record == null) return Guid.Empty;

        // Audit Log
        context.AdminLogs.Add(new AdminLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = adminUserId,
            Action = $"Deleted verification record ({record.DocumentType} for User {record.UserId})",
            TargetEntity = "VerificationRecord",
            TargetEntityId = record.Id,
            Timestamp = DateTime.UtcNow
        });

        context.VerificationRecords.Remove(record);
        await context.SaveChangesAsync(ct);
        return record.Id;
    }

    public async Task<UserVerificationProfileDto?> GetUserVerificationProfileAsync(Guid userId, CancellationToken ct)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user == null)
        {
            var prof = await context.ProfessionalProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == userId, ct);
            if (prof != null)
            {
                user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == prof.UserId, ct);
            }
        }

        if (user == null) return null;

        var profile = await context.ProfessionalProfiles
            .AsNoTracking()
            .Include(p => p.Certificates)
            .FirstOrDefaultAsync(p => p.UserId == user.Id, ct);

        var records = await context.VerificationRecords
            .AsNoTracking()
            .Where(vr => vr.UserId == user.Id)
            .OrderByDescending(vr => vr.VerificationDate)
            .ToListAsync(ct);

        var isVerified = (profile?.IsVerified ?? false) || records.Any(r => r.IsVerified);
        var latestVerificationDate = records.Where(r => r.IsVerified && r.VerificationDate.HasValue)
            .Select(r => r.VerificationDate)
            .FirstOrDefault();

        string badge = isVerified 
            ? "Verified Professional" 
            : (records.Any() ? "Pending Verification" : "Unverified");

        var certificates = profile?.Certificates.Select(c => new CertificateDto
        {
            Id = c.Id,
            ProfessionalProfileId = c.ProfessionalProfileId,
            Title = c.Title,
            Organization = c.Organization,
            IssueDate = c.IssueDate,
            ExpiryDate = c.ExpiryDate,
            DocumentImageUrl = c.DocumentImageUrl
        }).ToList() ?? new List<CertificateDto>();

        return new UserVerificationProfileDto
        {
            UserId = user.Id,
            FullName = $"{user.FirstName} {user.LastName}".Trim(),
            Email = user.Email ?? string.Empty,
            Phone = user.Phone,
            Role = user.Role.ToString(),
            Status = user.Status.ToString(),
            IsVerified = isVerified,
            VerificationBadge = badge,
            VerificationDate = latestVerificationDate,
            ProfilePhotoUrl = user.ProfilePhotoUrl,
            ProfessionalProfileId = profile?.Id,
            Headline = profile?.Headline,
            OverallRating = profile?.OverallRating ?? 0.00m,
            CompletedJobsCount = profile?.CompletedJobsCount ?? 0,
            YearsOfExperience = profile?.YearsOfExperience ?? 0,
            Documents = records.Select(r => new VerificationItemDto
            {
                Id = r.Id,
                DocumentType = r.DocumentType,
                DocumentUrl = r.DocumentUrl,
                IsVerified = r.IsVerified,
                VerificationDate = r.VerificationDate
            }).ToList(),
            Certificates = certificates
        };
    }
}
