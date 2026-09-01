using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Admin.Queries;

public class GetPendingProfessionalsHandler(IAdminService adminService)
    : IRequestHandler<GetPendingProfessionalsQuery, IEnumerable<object>>
{
    public async Task<IEnumerable<object>> Handle(GetPendingProfessionalsQuery request, CancellationToken ct)
    {
        return await adminService.GetPendingProfessionalsAsync(ct);
    }
}