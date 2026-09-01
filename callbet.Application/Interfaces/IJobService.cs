using callbet.Application.DTOs;

namespace callbet.Application.Interfaces;

public interface IJobService
{
    Task<Guid> CreateJobAsync(JobDto dto, CancellationToken ct);
    Task<Guid> AssignProfessionalAsync(Guid jobId, Guid professionalId, CancellationToken ct);
    Task<Guid> StartJobAsync(Guid jobId, CancellationToken ct);
    Task<Guid> CompleteJobAsync(Guid jobId, CancellationToken ct);
    Task<Guid> CloseJobAsync(Guid jobId, CancellationToken ct);
    Task<Guid> CreatePaymentAsync(PaymentDto dto, CancellationToken ct);
    Task<Guid> HoldPaymentAsync(Guid jobId, CancellationToken ct);
    Task<Guid> ReleasePaymentAsync(Guid jobId, CancellationToken ct);
    Task<Guid> CreateReviewAsync(ReviewDto dto, CancellationToken ct);
    Task<Guid> ReplyToReviewAsync(ReviewReplyDto dto, CancellationToken ct);
    Task<IEnumerable<ReviewDto>> GetReviewsForProfessionalAsync(Guid professionalId, CancellationToken ct);
    Task<ReviewDto?> GetReviewByJobIdAsync(Guid jobId, CancellationToken ct);
    Task<bool> DeleteReviewAsync(Guid reviewId, CancellationToken ct);
    Task<IEnumerable<object>> GetJobsForProfessionalAsync(Guid professionalId, CancellationToken ct);
    Task<IEnumerable<object>> GetPaymentsForProfessionalAsync(Guid professionalId, CancellationToken ct);
}