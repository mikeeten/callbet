using MediatR;
using callbet.Application.DTOs;
using callbet.Application.Interfaces;

namespace callbet.Application.Jobs.Queries;

public class GetReviewByJobIdHandler(IJobService service)
    : IRequestHandler<GetReviewByJobIdQuery, ReviewDto?>
{
    public async Task<ReviewDto?> Handle(GetReviewByJobIdQuery request, CancellationToken ct)
    {
        return await service.GetReviewByJobIdAsync(request.JobId, ct);
    }
}
