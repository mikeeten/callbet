using System.Threading;
using System.Threading.Tasks;
using callbet.Application.Interfaces;
using MediatR;

namespace callbet.Application.Admin.Commands;

public class ToggleVerifyProfessionalHandler(IAdminService adminService)
    : IRequestHandler<ToggleVerifyProfessionalCommand, bool>
{
    public async Task<bool> Handle(ToggleVerifyProfessionalCommand request, CancellationToken cancellationToken)
    {
        return await adminService.ToggleVerifyProfessionalAsync(request.UserId, request.AdminUserId, cancellationToken);
    }
}
