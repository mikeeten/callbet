using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Admin.Queries;

public class GetProfessionalServicesHandler(IAdminService adminService)
    : IRequestHandler<GetProfessionalServicesQuery, IEnumerable<object>>
{
    public async Task<IEnumerable<object>> Handle(GetProfessionalServicesQuery request, CancellationToken ct)
    {
        return await adminService.GetProfessionalServicesAsync(request.ProfileId, ct);
    }
}