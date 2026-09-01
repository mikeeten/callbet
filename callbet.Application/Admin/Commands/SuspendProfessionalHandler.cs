using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Admin.Commands;

public class SuspendProfessionalHandler(IAdminService adminService)
    : IRequestHandler<SuspendProfessionalCommand, Guid>
{
    public async Task<Guid> Handle(SuspendProfessionalCommand request, CancellationToken ct)
    {
        return await adminService.SuspendProfessionalAsync(request.UserId, ct);
    }
}