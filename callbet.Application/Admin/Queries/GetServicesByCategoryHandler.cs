using MediatR;
using callbet.Domain.Entities;
using callbet.Application.Interfaces;

namespace callbet.Application.Admin.Queries;

public class GetServicesByCategoryHandler(IAdminService adminService)
    : IRequestHandler<GetServicesByCategoryQuery, IEnumerable<Service>>
{
    public async Task<IEnumerable<Service>> Handle(GetServicesByCategoryQuery request, CancellationToken ct)
    {
        return await adminService.GetServicesByCategoryAsync(request.CategoryId, ct);
    }
}