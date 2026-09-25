using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Admin.Commands;

public class DeleteServiceZoneHandler(IAdminService adminService)
    : IRequestHandler<DeleteServiceZoneCommand, bool>
{
    public async Task<bool> Handle(DeleteServiceZoneCommand request, CancellationToken ct)
    {
        return await adminService.DeleteServiceZoneAsync(request.NeighborhoodId, request.AdminUserId, ct);
    }
}
