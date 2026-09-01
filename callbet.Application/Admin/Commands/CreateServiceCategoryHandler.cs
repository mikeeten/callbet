using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Admin.Commands;

public class CreateServiceCategoryHandler(IAdminService adminService)
    : IRequestHandler<CreateServiceCategoryCommand, Guid>
{
    public async Task<Guid> Handle(CreateServiceCategoryCommand request, CancellationToken ct)
    {
        return await adminService.CreateServiceCategoryAsync(request.Dto, ct);
    }
}