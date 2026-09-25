using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Admin.Commands;

public class DismissReviewFlagHandler(IAdminService adminService)
    : IRequestHandler<DismissReviewFlagCommand, bool>
{
    public async Task<bool> Handle(DismissReviewFlagCommand request, CancellationToken ct)
    {
        return await adminService.DismissReviewFlagAsync(request.ReviewId, request.AdminUserId, ct);
    }
}
