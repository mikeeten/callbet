using MediatR;
using callbet.Application.DTOs;
using callbet.Application.Interfaces;

namespace callbet.Application.Admin.Commands;

public class CreateServiceZoneHandler(IAdminService adminService)
    : IRequestHandler<CreateServiceZoneCommand, ServiceZoneDto>
{
    public async Task<ServiceZoneDto> Handle(CreateServiceZoneCommand request, CancellationToken ct)
    {
        return await adminService.CreateServiceZoneAsync(request.Dto, request.AdminUserId, ct);
    }
}
