using callbet.Application.DTOs;
using callbet.Application.Interfaces;
using callbet.Domain.Entities;
using callbet.Infrastructure.Hubs;
using callbet.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace callbet.Infrastructure.Services;

public class JobService(
    CallbetDbContext context,
    IHubContext<ChatHub, IChatHubClient> hubContext) : IJobService
{
    private async Task AddAndBroadcastNotificationAsync(Guid targetId, string message, string? link = null)
    {
        if (targetId == Guid.Empty) return;

        // Resolve actual AspNetUsers.Id if targetId is a ProfessionalProfile.Id
        var targetUserId = targetId;
        var proProfile = await context.ProfessionalProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == targetId);

        if (proProfile != null && proProfile.UserId != Guid.Empty)
        {
            targetUserId = proProfile.UserId;
        }

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = targetUserId,
            Message = message,
            Link = link,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        context.Notifications.Add(notification);

        var dto = new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Message = notification.Message,
            Link = notification.Link,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        };

        // Broadcast to both group channels so the subscriber receives the live alert instantly
        await hubContext.Clients.Group($"user_{targetUserId}").ReceiveNotification(dto);
        if (targetId != targetUserId)
        {
            await hubContext.Clients.Group($"user_{targetId}").ReceiveNotification(dto);
        }
    }

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
            await AddAndBroadcastNotificationAsync(dto.ProfessionalId, "New Job Request: A customer has booked your service.", $"/jobs/{job.Id}");
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
        await AddAndBroadcastNotificationAsync(job.CustomerId, "Professional Assigned: Your job has been assigned to a professional.", $"/jobs/{job.Id}");

        // Notify Professional
        await AddAndBroadcastNotificationAsync(professionalId, "New Job Assigned: You have been assigned to a new job.", $"/jobs/{job.Id}");

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
        await AddAndBroadcastNotificationAsync(job.CustomerId, "Job In Progress: The professional has started working on your job.", $"/jobs/{job.Id}");

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
        await AddAndBroadcastNotificationAsync(job.CustomerId, "Job Completed: The professional marked the job as completed. Please review and approve payment!", $"/jobs/{job.Id}/review");

        await context.SaveChangesAsync(ct);
        return job.Id;
    }

    public async Task<bool> CancelJobAsync(Guid jobId, Guid callerUserId = default, CancellationToken ct = default)
    {
        var job = await context.Jobs.FindAsync(new object[] { jobId }, ct);
        if (job == null) return false;

        job.Status = JobStatus.Cancelled;
        job.UpdatedAt = DateTime.UtcNow;

        // Fetch service name for descriptive notifications
        var service = await context.Services.FindAsync(new object[] { job.ServiceId }, ct);
        var serviceName = service != null ? service.Name : "Service";

        // Determine who cancelled the job
        bool isCustomerCaller = callerUserId != Guid.Empty && callerUserId == job.CustomerId;
        bool isProCaller = callerUserId != Guid.Empty && (callerUserId == job.ProfessionalId || await context.ProfessionalProfiles.AnyAsync(p => p.UserId == callerUserId && p.Id == job.ProfessionalId, ct));

        if (isCustomerCaller)
        {
            if (job.CustomerId != Guid.Empty)
            {
                await AddAndBroadcastNotificationAsync(job.CustomerId, $"Booking Cancelled: You have cancelled your booking for \"{serviceName}\". Escrow funds will be refunded.", $"/customer-booked-services");
            }
            if (job.ProfessionalId != Guid.Empty)
            {
                await AddAndBroadcastNotificationAsync(job.ProfessionalId, $"Booking Cancelled by Customer: The customer cancelled their booking for \"{serviceName}\".", $"/jobs");
            }
        }
        else if (isProCaller)
        {
            if (job.ProfessionalId != Guid.Empty)
            {
                await AddAndBroadcastNotificationAsync(job.ProfessionalId, $"Job Assignment Cancelled: You have declined/cancelled your assignment for \"{serviceName}\".", $"/jobs");
            }
            if (job.CustomerId != Guid.Empty)
            {
                await AddAndBroadcastNotificationAsync(job.CustomerId, $"Job Cancelled by Professional: The assigned specialist declined/cancelled the job for \"{serviceName}\". You can reassign or request a refund.", $"/customer-booked-services");
            }
        }
        else
        {
            if (job.CustomerId != Guid.Empty)
            {
                await AddAndBroadcastNotificationAsync(job.CustomerId, $"Job Booking Cancelled: The booking for \"{serviceName}\" was cancelled.", $"/customer-booked-services");
            }
            if (job.ProfessionalId != Guid.Empty)
            {
                await AddAndBroadcastNotificationAsync(job.ProfessionalId, $"Job Booking Cancelled: The booking for \"{serviceName}\" was cancelled.", $"/jobs");
            }
        }

        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteJobAsync(Guid jobId, Guid callerUserId = default, CancellationToken ct = default)
    {
        var job = await context.Jobs.FindAsync(new object[] { jobId }, ct);
        if (job == null) return false;

        context.Jobs.Remove(job);
        await context.SaveChangesAsync(ct);
        return true;
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
            await AddAndBroadcastNotificationAsync(job.ProfessionalId, "Job Approved & Closed: The customer has approved the completed job.", $"/jobs/{job.Id}");
        }

        await context.SaveChangesAsync(ct);
        return job.Id;
    }

    public async Task<IEnumerable<JobDto>> GetMyJobsAsync(Guid userId, CancellationToken ct)
    {
        return await context.Jobs
            .Where(j => j.CustomerId == userId || j.ProfessionalId == userId)
            .OrderByDescending(j => j.CreatedAt)
            .Select(j => new JobDto
            {
                Id = j.Id,
                CustomerId = j.CustomerId,
                ProfessionalId = j.ProfessionalId,
                ServiceId = j.ServiceId,
                Description = j.Description,
                AddressId = j.AddressId,
                ScheduledDateTime = j.ScheduledDateTime,
                EstimatedDurationMins = j.EstimatedDurationMins,
                Price = j.Price,
                Status = j.Status
            })
            .ToListAsync(ct);
    }

    public async Task<JobDto?> GetJobDetailsAsync(Guid jobId, CancellationToken ct)
    {
        var j = await context.Jobs.FindAsync(new object[] { jobId }, ct);
        if (j == null) return null;

        return new JobDto
        {
            Id = j.Id,
            CustomerId = j.CustomerId,
            ProfessionalId = j.ProfessionalId,
            ServiceId = j.ServiceId,
            Description = j.Description,
            AddressId = j.AddressId,
            ScheduledDateTime = j.ScheduledDateTime,
            EstimatedDurationMins = j.EstimatedDurationMins,
            Price = j.Price,
            Status = j.Status
        };
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
        await AddAndBroadcastNotificationAsync(dto.RevieweeId, "You have received a new review from a customer.", $"/reviews/{review.Id}");

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
            await AddAndBroadcastNotificationAsync(review.ReviewerId, "The professional has replied to your review.");
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

    public async Task<bool> DeleteReviewAsync(Guid reviewId, Guid adminUserId = default, CancellationToken ct = default)
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
            AdminUserId = adminUserId,
            Action = $"Removed review (Rating: {review.Rating}★) for user {review.RevieweeId}",
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

    public async Task<IEnumerable<object>> GetJobsForCustomerAsync(Guid customerId, CancellationToken ct)
    {
        var rawList = await (
            from j in context.Jobs
            where j.CustomerId == customerId
            join s in context.Services on j.ServiceId equals s.Id into services
            from s in services.DefaultIfEmpty()
            join proProfile in context.ProfessionalProfiles on j.ProfessionalId equals proProfile.Id into proProfiles
            from pp in proProfiles.DefaultIfEmpty()
            join proUser in context.Users on (pp != null ? pp.UserId : j.ProfessionalId) equals proUser.Id into proUsers
            from pu in proUsers.DefaultIfEmpty()
            join a in context.Addresses on j.AddressId equals a.Id into addrs
            from addr in addrs.DefaultIfEmpty()
            orderby j.CreatedAt descending
            select new
            {
                j.Id,
                ServiceId = s != null ? s.Id : (j.ServiceId != Guid.Empty ? j.ServiceId : Guid.Empty),
                ServiceName = s != null ? s.Name : "Home Service",
                ProfessionalName = pu != null ? (pu.FirstName + " " + pu.LastName).Trim() : "Assigned Specialist",
                ProfessionalUserId = pu != null ? pu.Id : (pp != null ? pp.UserId : j.ProfessionalId),
                ProfessionalProfileId = pp != null ? pp.Id : j.ProfessionalId,
                ProfessionalAvatar = pu != null ? pu.ProfilePhotoUrl : null,
                j.CustomerId,
                j.ProfessionalId,
                Status = j.Status,
                j.Price,
                j.ScheduledDateTime,
                Street = addr != null ? addr.Street : null,
                City = addr != null ? addr.City : null,
                j.Description,
                j.CreatedAt
            }
        ).ToListAsync(ct);

        return rawList.Select(x => new
        {
            x.Id,
            x.ServiceId,
            x.ServiceName,
            x.ProfessionalName,
            ProfessionalUserId = x.ProfessionalUserId,
            ProfessionalProfileId = x.ProfessionalProfileId,
            ProfessionalAvatar = x.ProfessionalAvatar,
            x.CustomerId,
            ProfessionalId = x.ProfessionalUserId,
            Status = x.Status.ToString(),
            x.Price,
            ScheduledDate = x.ScheduledDateTime.ToString("yyyy-MM-dd hh:mm tt"),
            Address = !string.IsNullOrEmpty(x.Street) ? $"{x.Street}, {x.City}" : "Addis Ababa Service Site",
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