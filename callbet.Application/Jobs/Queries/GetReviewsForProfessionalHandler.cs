using MediatR;
using callbet.Application.DTOs;
using callbet.Application.Interfaces;

namespace callbet.Application.Jobs.Queries;

public class GetReviewsForProfessionalHandler(IJobService service)
    : IRequestHandler<GetReviewsForProfessionalQuery, IEnumerable<ReviewDto>>
{
    public async Task<IEnumerable<ReviewDto>> Handle(GetReviewsForProfessionalQuery request, CancellationToken ct)
    {
        return await service.GetReviewsForProfessionalAsync(request.ProfessionalId, ct);
    }
}
