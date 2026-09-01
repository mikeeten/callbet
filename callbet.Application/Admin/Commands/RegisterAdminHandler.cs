using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Admin.Commands;

public class RegisterAdminHandler(IAdminService adminService)
    : IRequestHandler<RegisterAdminCommand, Guid>
{
    public async Task<Guid> Handle(RegisterAdminCommand request, CancellationToken ct)
    {
        return await adminService.RegisterAdminAsync(request.Dto, ct);
    }
}