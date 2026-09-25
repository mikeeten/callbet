using MediatR;
using callbet.Application.DTOs;
using callbet.Application.Interfaces;

namespace callbet.Application.Professionals.Queries;

public class GetProfessionalProfileDashboardHandler(IProfessionalService professionalService)
    : IRequestHandler<GetProfessionalProfileDashboardQuery, GetProfessionalProfileDashboardDto?>
{
    public async Task<GetProfessionalProfileDashboardDto?> Handle(GetProfessionalProfileDashboardQuery request, CancellationToken ct)
    {
        return await professionalService.GetProfessionalProfileDashboardAsync(request.Id, ct);
    }
}
