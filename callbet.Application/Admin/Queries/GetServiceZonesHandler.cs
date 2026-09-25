using MediatR;
using callbet.Application.DTOs;
using callbet.Application.Interfaces;

namespace callbet.Application.Admin.Queries;

public class GetServiceZonesHandler(IAdminService adminService)
    : IRequestHandler<GetServiceZonesQuery, IEnumerable<ServiceZoneDto>>
{
    public async Task<IEnumerable<ServiceZoneDto>> Handle(GetServiceZonesQuery request, CancellationToken ct)
    {
        return await adminService.GetServiceZonesAsync(ct);
    }
}
