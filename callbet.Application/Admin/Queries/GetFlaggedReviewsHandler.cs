using MediatR;
using callbet.Application.DTOs;
using callbet.Application.Interfaces;

namespace callbet.Application.Admin.Queries;

public class GetFlaggedReviewsHandler(IAdminService adminService)
    : IRequestHandler<GetFlaggedReviewsQuery, PagedResponse<FlaggedReviewAdminDto>>
{
    public async Task<PagedResponse<FlaggedReviewAdminDto>> Handle(GetFlaggedReviewsQuery request, CancellationToken ct)
    {
        return await adminService.GetFlaggedReviewsAsync(request.Request, ct);
    }
}
