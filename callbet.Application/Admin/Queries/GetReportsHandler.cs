using MediatR;
using callbet.Application.DTOs;
using callbet.Application.Interfaces;

namespace callbet.Application.Admin.Queries;

public class GetReportsHandler(IAdminService adminService)
    : IRequestHandler<GetReportsQuery, PagedResponse<AdminReportDto>>
{
    public async Task<PagedResponse<AdminReportDto>> Handle(GetReportsQuery request, CancellationToken ct)
    {
        return await adminService.GetReportsAsync(request.Request, ct);
    }
}
