using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Jobs.Queries;

public class GetPaymentsForProfessionalHandler(IJobService jobService)
    : IRequestHandler<GetPaymentsForProfessionalQuery, IEnumerable<object>>
{
    public async Task<IEnumerable<object>> Handle(GetPaymentsForProfessionalQuery request, CancellationToken ct)
    {
        return await jobService.GetPaymentsForProfessionalAsync(request.ProfessionalId, ct);
    }
}
