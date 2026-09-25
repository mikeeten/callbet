using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Admin.Commands;

public class ActivateUserHandler(IAdminService adminService)
    : IRequestHandler<ActivateUserCommand, Guid>
{
    public async Task<Guid> Handle(ActivateUserCommand request, CancellationToken ct)
    {
        return await adminService.ActivateUserAsync(request.UserId, request.AdminUserId, ct);
    }
}
