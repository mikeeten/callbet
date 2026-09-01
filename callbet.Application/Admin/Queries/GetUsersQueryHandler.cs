using callbet.Application.DTOs;
using callbet.Application.Interfaces;
using callbet.Domain.Entities;
using MediatR;

namespace callbet.Application.Admin.Queries;

public class GetUsersQueryHandler(IAdminService adminService) : IRequestHandler<GetUsersQuery, PagedResponse<UserResponseDto>>
{
    public async Task<PagedResponse<UserResponseDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        return await adminService.GetUsersAsync(request.Request, cancellationToken);
    }
}