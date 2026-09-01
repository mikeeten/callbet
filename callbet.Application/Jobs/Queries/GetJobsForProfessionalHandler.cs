using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Jobs.Queries;

public class GetJobsForProfessionalHandler(IJobService jobService)
    : IRequestHandler<GetJobsForProfessionalQuery, IEnumerable<object>>
{
    public async Task<IEnumerable<object>> Handle(GetJobsForProfessionalQuery request, CancellationToken ct)
    {
        return await jobService.GetJobsForProfessionalAsync(request.ProfessionalId, ct);
    }
}
