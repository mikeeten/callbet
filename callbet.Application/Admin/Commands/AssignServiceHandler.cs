using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Admin.Commands;

public class AssignServiceHandler(IAdminService adminService)
    : IRequestHandler<AssignServiceCommand, Guid>
{
    public async Task<Guid> Handle(AssignServiceCommand request, CancellationToken ct)
    {
        return await adminService.AssignServiceAsync(request.Dto, ct);
    }
}