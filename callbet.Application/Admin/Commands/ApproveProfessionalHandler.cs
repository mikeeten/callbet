using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Admin.Commands;

public class ApproveProfessionalHandler(IAdminService adminService)
    : IRequestHandler<ApproveProfessionalCommand, Guid>
{
    public async Task<Guid> Handle(ApproveProfessionalCommand request, CancellationToken ct)
    {
        return await adminService.ApproveProfessionalAsync(request.UserId, request.AdminUserId, ct);
    }
}