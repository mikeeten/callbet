using System.Threading;
using System.Threading.Tasks;
using callbet.Application.DTOs;
using callbet.Application.Interfaces;
using MediatR;

namespace callbet.Application.Admin.Queries;

public class GetProfessionalProfilesQueryHandler(IAdminService adminService)
    : IRequestHandler<GetProfessionalProfilesQuery, PagedResponse<ProfessionalProfileAdminDto>>
{
    public async Task<PagedResponse<ProfessionalProfileAdminDto>> Handle(GetProfessionalProfilesQuery request, CancellationToken cancellationToken)
    {
        return await adminService.GetProfessionalProfilesAsync(request.Request, cancellationToken);
    }
}
