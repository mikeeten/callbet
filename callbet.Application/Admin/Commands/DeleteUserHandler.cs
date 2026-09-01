using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Admin.Commands;

public class DeleteUserHandler(IAdminService adminService)
    : IRequestHandler<DeleteUserCommand, Guid>
{
    public async Task<Guid> Handle(DeleteUserCommand request, CancellationToken ct)
    {
        return await adminService.DeleteUserAsync(request.UserId, ct);
    }
}