using callbet.Application.DTOs;
using callbet.Application.Interfaces;
using callbet.Domain.Entities;
using callbet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace callbet.Infrastructure.Services;

public class VerificationService(CallbetDbContext context) : IVerificationService
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

    public async Task<Guid> ApproveVerificationAsync(Guid recordId, CancellationToken ct)
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
        context.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = record.UserId,
            Message = "Your verification document has been approved! Your account is now verified.",
            Link = "/profile",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        // Audit Log
        context.AdminLogs.Add(new AdminLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = Guid.Empty,
            Action = "Approved professional verification",
            TargetEntity = "VerificationRecord",
            TargetEntityId = record.Id,
            Timestamp = DateTime.UtcNow
        });

        await context.SaveChangesAsync(ct);
        return record.Id;
    }

    public async Task<Guid> DeleteVerificationAsync(Guid recordId, CancellationToken ct)
    {
        var record = await context.VerificationRecords.FindAsync(new object[] { recordId }, ct);
        if (record == null) return Guid.Empty;

        context.VerificationRecords.Remove(record);
        await context.SaveChangesAsync(ct);
        return record.Id;
    }
}
