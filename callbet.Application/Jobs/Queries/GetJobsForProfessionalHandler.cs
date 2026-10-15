using callbet.Application.DTOs;
using callbet.Application.Interfaces;
using MediatR;

namespace callbet.Application.Jobs.Queries;

public class GetJobsForProfessionalHandler(IJobService jobService)
    : IRequestHandler<GetJobsForProfessionalQuery, PagedResponse<ProfessionalJobDto>>
{
    public async Task<PagedResponse<ProfessionalJobDto>> Handle(GetJobsForProfessionalQuery request, CancellationToken ct)
    {
        var pagedReq = request.Request ?? new PagedRequest();
        return await jobService.GetJobsForProfessionalPagedAsync(request.ProfessionalId, pagedReq, ct);
    }
}
