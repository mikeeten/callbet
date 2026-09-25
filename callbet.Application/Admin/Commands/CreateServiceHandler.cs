using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Admin.Commands;

public class CreateServiceHandler(IAdminService adminService)
    : IRequestHandler<CreateServiceCommand, Guid>
{
    public async Task<Guid> Handle(CreateServiceCommand request, CancellationToken ct)
    {
        return await adminService.CreateServiceAsync(request.Dto, request.AdminUserId, ct);
    }
}