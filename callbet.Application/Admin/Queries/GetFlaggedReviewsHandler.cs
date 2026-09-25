using MediatR;
using callbet.Application.DTOs;
using callbet.Application.Interfaces;

namespace callbet.Application.Admin.Queries;

public class GetFlaggedReviewsHandler(IAdminService adminService)
    : IRequestHandler<GetFlaggedReviewsQuery, IEnumerable<FlaggedReviewAdminDto>>
{
    public async Task<IEnumerable<FlaggedReviewAdminDto>> Handle(GetFlaggedReviewsQuery request, CancellationToken ct)
    {
        return await adminService.GetFlaggedReviewsAsync(ct);
    }
}
