using MediatR;
using callbet.Application.DTOs;
using callbet.Application.Interfaces;

namespace callbet.Application.Professionals.Queries;

public class GetProfessionalProfileDetailsHandler(IProfessionalService professionalService)
    : IRequestHandler<GetProfessionalProfileDetailsQuery, ProfessionalProfileDetailsDto?>
{
    public async Task<ProfessionalProfileDetailsDto?> Handle(GetProfessionalProfileDetailsQuery request, CancellationToken ct)
    {
        return await professionalService.GetProfileDetailsAsync(request.Id, ct);
    }
}
