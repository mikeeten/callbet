using MediatR;
using callbet.Domain.Entities;
using callbet.Application.Interfaces;

namespace callbet.Application.Admin.Queries;

public class GetServiceCategoriesHandler(IAdminService adminService)
    : IRequestHandler<GetServiceCategoriesQuery, IEnumerable<ServiceCategory>>
{
    public async Task<IEnumerable<ServiceCategory>> Handle(GetServiceCategoriesQuery request, CancellationToken ct)
    {
        return await adminService.GetServiceCategoriesAsync(ct);
    }
}