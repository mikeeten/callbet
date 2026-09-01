using callbet.Application.DTOs;
using callbet.Application.Interfaces;
using callbet.Domain.Entities;
using callbet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace callbet.Infrastructure.Services;

public class JobService(CallbetDbContext context) : IJobService
{
    // Jobs
    public async Task<Guid> CreateJobAsync(JobDto dto, CancellationToken ct)
    {
        var job = new Job
        {
            Id = dto.Id != Guid.Empty ? dto.Id : Guid.NewGuid(),
            CustomerId = dto.CustomerId,
            ProfessionalId = dto.ProfessionalId,
            ServiceId = dto.ServiceId,
            Description = dto.Description,
            AddressId = dto.AddressId,
            ScheduledDateTime = dto.ScheduledDateTime != default ? dto.ScheduledDateTime : DateTime.UtcNow,
            EstimatedDurationMins = dto.EstimatedDurationMins,
            Price = dto.Price,
            Status = dto.ProfessionalId != Guid.Empty ? JobStatus.Assigned : JobStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Jobs.Add(job);

        // Notify Professional if directly booked
        if (dto.ProfessionalId != Guid.Empty)
        {
            context.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = dto.ProfessionalId,
                Message = "New Job Request: A customer has booked your service.",
                Link = $"/jobs/{job.Id}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync(ct);
        return job.Id;
    }

    public async Task<Guid> AssignProfessionalAsync(Guid jobId, Guid professionalId, CancellationToken ct)
    {
        var job = await context.Jobs.FindAsync(new object[] { jobId }, ct);
        if (job == null) return Guid.Empty;

        job.ProfessionalId = professionalId;
        job.Status = JobStatus.Assigned;
        job.UpdatedAt = DateTime.UtcNow;

        // Notify Customer
        context.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = job.CustomerId,
            Message = "Professional Assigned: Your job has been assigned to a professional.",
            Link = $"/jobs/{job.Id}",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        // Notify Professional
        context.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = professionalId,
            Message = "New Job Assigned: You have been assigned to a new job.",
            Link = $"/jobs/{job.Id}",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync(ct);
        return job.Id;
    }

    public async Task<Guid> StartJobAsync(Guid jobId, CancellationToken ct)
    {
        var job = await context.Jobs.FindAsync(new object[] { jobId }, ct);
        if (job == null) return Guid.Empty;

        job.Status = JobStatus.InProgress;
        job.UpdatedAt = DateTime.UtcNow;

        // Notify Customer
        context.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = job.CustomerId,
            Message = "Job In Progress: The professional has started working on your job.",
            Link = $"/jobs/{job.Id}",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync(ct);
        return job.Id;
    }

    public async Task<Guid> CompleteJobAsync(Guid jobId, CancellationToken ct)
    {
        var job = await context.Jobs.FindAsync(new object[] { jobId }, ct);
        if (job == null) return Guid.Empty;

        job.Status = JobStatus.CompletedPendingApproval;
        job.UpdatedAt = DateTime.UtcNow;

        // Notify Customer to review and approve
        context.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = job.CustomerId,
            Message = "Job Completed: The professional marked the job as completed. Please review and approve payment!",
            Link = $"/jobs/{job.Id}/review",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync(ct);
        return job.Id;
    }

    public async Task<Guid> CloseJobAsync(Guid jobId, CancellationToken ct)
    {
        var job = await context.Jobs.FindAsync(new object[] { jobId }, ct);
        if (job == null) return Guid.Empty;

        job.Status = JobStatus.Closed;
        job.UpdatedAt = DateTime.UtcNow;

        // Notify Professional that the job is closed and approved
        if (job.ProfessionalId != Guid.Empty)
        {
            context.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = job.ProfessionalId,
                Message = "Job Approved & Closed: The customer has approved the completed job.",
                Link = $"/jobs/{job.Id}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync(ct);
        return job.Id;
    }

    // Payments
    public async Task<Guid> CreatePaymentAsync(PaymentDto dto, CancellationToken ct)
    {
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            JobId = dto.JobId,
            Amount = dto.Amount,
            Status = PaymentStatus.Pending,
            TransactionId = dto.TransactionId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Payments.Add(payment);
        await context.SaveChangesAsync(ct);
        return payment.Id;
    }

    public async Task<Guid> HoldPaymentAsync(Guid jobId, CancellationToken ct)
    {
        var payment = await context.Payments.FirstOrDefaultAsync(p => p.JobId == jobId, ct);
        if (payment == null) return Guid.Empty;

        payment.Status = PaymentStatus.HeldInEscrow;
        payment.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);
        return payment.Id;
    }

    public async Task<Guid> ReleasePaymentAsync(Guid jobId, CancellationToken ct)
    {
        var payment = await context.Payments.FirstOrDefaultAsync(p => p.JobId == jobId, ct);
        if (payment == null) return Guid.Empty;

        payment.Status = PaymentStatus.ReleasedToWorker;
        payment.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);
        return payment.Id;
    }

    // Reviews & Reputation
    public async Task<Guid> CreateReviewAsync(ReviewDto dto, CancellationToken ct)
    {
        // 1. Insert review
        var review = new Review
        {
            Id = dto.Id != Guid.Empty ? dto.Id : Guid.NewGuid(),
            JobId = dto.JobId,
            ReviewerId = dto.ReviewerId,
            RevieweeId = dto.RevieweeId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            CreatedAt = DateTime.UtcNow
        };

        context.Reviews.Add(review);

        // 2. Update Professional Profile Rating & Increment Completed Jobs Count
        var profile = await context.ProfessionalProfiles
            .FirstOrDefaultAsync(p => p.UserId == dto.RevieweeId || p.Id == dto.RevieweeId, ct);
        if (profile != null)
        {
            var existingRatings = await context.Reviews
                .Where(r => r.RevieweeId == profile.UserId && r.Id != review.Id)
                .Select(r => (decimal)r.Rating)
                .ToListAsync(ct);
            existingRatings.Add((decimal)dto.Rating);

            profile.OverallRating = Math.Round(existingRatings.Average(), 2);
            profile.CompletedJobsCount += 1;
            profile.UpdatedAt = DateTime.UtcNow;
        }

        // 3. Notification to professional
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = dto.RevieweeId,
            Message = "You have received a new review from a customer.",
            Link = $"/reviews/{review.Id}",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        context.Notifications.Add(notification);

        await context.SaveChangesAsync(ct);
        return review.Id;
    }

    public async Task<Guid> ReplyToReviewAsync(ReviewReplyDto dto, CancellationToken ct)
    {
        // 1. Insert review reply
        var reply = new ReviewReply
        {
            Id = Guid.NewGuid(),
            ReviewId = dto.ReviewId,
            ReplierId = dto.ReplierId,
            Comment = dto.Comment,
            CreatedAt = DateTime.UtcNow
        };

        context.ReviewReplies.Add(reply);

        // 2. Notification to customer
        var review = await context.Reviews.FindAsync(new object[] { dto.ReviewId }, ct);
        if (review != null)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = review.ReviewerId,
                Message = "The professional has replied to your review.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            context.Notifications.Add(notification);
        }

        await context.SaveChangesAsync(ct);
        return reply.Id;
    }

    public async Task<IEnumerable<ReviewDto>> GetReviewsForProfessionalAsync(Guid professionalId, CancellationToken ct)
    {
        return await context.Reviews
            .Where(r => r.RevieweeId == professionalId)
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
                      CustomerName = u.FirstName,
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
    }

    public async Task<ReviewDto?> GetReviewByJobIdAsync(Guid jobId, CancellationToken ct)
    {
        var r = await context.Reviews
            .Include(x => x.Reply)
            .FirstOrDefaultAsync(x => x.JobId == jobId, ct);

        if (r == null) return null;

        return new ReviewDto
        {
            Id = r.Id,
            JobId = r.JobId,
            ReviewerId = r.ReviewerId,
            RevieweeId = r.RevieweeId,
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt,
            Reply = r.Reply == null ? null : new ReviewReplyDto
            {
                Id = r.Reply.Id,
                ReviewId = r.Reply.ReviewId,
                ReplierId = r.Reply.ReplierId,
                Comment = r.Reply.Comment,
                CreatedAt = r.Reply.CreatedAt
            }
        };
    }

    public async Task<bool> DeleteReviewAsync(Guid reviewId, CancellationToken ct)
    {
        var review = await context.Reviews
            .Include(r => r.Reply)
            .FirstOrDefaultAsync(r => r.Id == reviewId, ct);

        if (review == null) return false;

        var professionalId = review.RevieweeId;

        if (review.Reply != null)
        {
            context.ReviewReplies.Remove(review.Reply);
        }

        context.Reviews.Remove(review);

        // Audit log entry for admin moderation
        context.AdminLogs.Add(new AdminLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = Guid.Empty,
            Action = "Removed abusive review",
            TargetEntity = "Review",
            TargetEntityId = reviewId,
            Timestamp = DateTime.UtcNow
        });

        // Recalculate remaining rating
        var profile = await context.ProfessionalProfiles
            .FirstOrDefaultAsync(p => p.UserId == professionalId || p.Id == professionalId, ct);
        if (profile != null)
        {
            var remainingRatings = await context.Reviews
                .Where(r => r.RevieweeId == profile.UserId && r.Id != reviewId)
                .Select(r => (decimal)r.Rating)
                .ToListAsync(ct);

            profile.OverallRating = remainingRatings.Count > 0 ? Math.Round(remainingRatings.Average(), 2) : 0.00m;
            profile.UpdatedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IEnumerable<object>> GetJobsForProfessionalAsync(Guid professionalId, CancellationToken ct)
    {
        var profile = await context.ProfessionalProfiles
            .FirstOrDefaultAsync(p => p.Id == professionalId || p.UserId == professionalId, ct);
        var targetUserId = profile != null ? profile.UserId : professionalId;

        var rawList = await context.Jobs
            .Where(j => j.ProfessionalId == targetUserId || j.ProfessionalId == professionalId)
            .Join(context.Users,
                  j => j.CustomerId,
                  u => u.Id,
                  (j, u) => new { j, u })
            .Join(context.Services,
                  ju => ju.j.ServiceId,
                  s => s.Id,
                  (ju, s) => new { ju.j, ju.u, s })
            .GroupJoin(context.Addresses,
                       jus => jus.j.AddressId,
                       a => a.Id,
                       (jus, addrs) => new { jus.j, jus.u, jus.s, addr = addrs.FirstOrDefault() })
            .OrderByDescending(x => x.j.CreatedAt)
            .Select(x => new
            {
                x.j.Id,
                ServiceName = x.s.Name,
                CustomerName = x.u.FirstName + " " + x.u.LastName,
                x.j.CustomerId,
                x.j.ProfessionalId,
                Status = x.j.Status,
                x.j.Price,
                x.j.ScheduledDateTime,
                Street = x.addr != null ? x.addr.Street : null,
                City = x.addr != null ? x.addr.City : null,
                x.j.Description,
                x.j.CreatedAt
            })
            .ToListAsync(ct);

        return rawList.Select(x => new
        {
            x.Id,
            x.ServiceName,
            x.CustomerName,
            x.CustomerId,
            x.ProfessionalId,
            Status = x.Status.ToString(),
            x.Price,
            NetPayout = Math.Round(x.Price * 0.85m, 2),
            PlatformFee = Math.Round(x.Price * 0.15m, 2),
            ScheduledDate = x.ScheduledDateTime.ToString("yyyy-MM-dd hh:mm tt"),
            Address = !string.IsNullOrEmpty(x.Street) ? $"{x.Street}, {x.City}" : "Location Provided Upon Booking",
            x.Description,
            x.CreatedAt
        });
    }

    public async Task<IEnumerable<object>> GetPaymentsForProfessionalAsync(Guid professionalId, CancellationToken ct)
    {
        var profile = await context.ProfessionalProfiles
            .FirstOrDefaultAsync(p => p.Id == professionalId || p.UserId == professionalId, ct);
        var targetUserId = profile != null ? profile.UserId : professionalId;

        var rawList = await context.Payments
            .Join(context.Jobs,
                  p => p.JobId,
                  j => j.Id,
                  (p, j) => new { p, j })
            .Where(pj => pj.j.ProfessionalId == targetUserId || pj.j.ProfessionalId == professionalId)
            .Join(context.Services,
                  pj => pj.j.ServiceId,
                  s => s.Id,
                  (pj, s) => new
                  {
                      pj.p.Id,
                      JobId = pj.j.Id,
                      ServiceName = s.Name,
                      GrossAmount = pj.p.Amount,
                      Status = pj.p.Status,
                      pj.p.CreatedAt
                  })
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

        return rawList.Select(x => new
        {
            x.Id,
            x.JobId,
            x.ServiceName,
            x.GrossAmount,
            NetPayout = Math.Round(x.GrossAmount * 0.85m, 2),
            PlatformFee = Math.Round(x.GrossAmount * 0.15m, 2),
            Status = x.Status == PaymentStatus.HeldInEscrow
                ? "Held in Escrow"
                : x.Status == PaymentStatus.ReleasedToWorker
                ? "Disbursed"
                : "Pending",
            Date = x.CreatedAt.ToString("yyyy-MM-dd")
        });
    }
}