using MediatR;
using callbet.Application.DTOs;
using callbet.Application.Interfaces;

namespace callbet.Application.Admin.Queries;

public class GetAdminLogsHandler(IAdminService adminService)
    : IRequestHandler<GetAdminLogsQuery, PagedResponse<AdminLogDto>>
{
    public async Task<PagedResponse<AdminLogDto>> Handle(GetAdminLogsQuery request, CancellationToken ct)
    {
        return await adminService.GetAdminLogsPagedAsync(request.Request, ct);
    }
}
