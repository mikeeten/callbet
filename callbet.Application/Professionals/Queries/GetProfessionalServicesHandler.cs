using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Professionals.Queries;

public class GetProfessionalServicesHandler(IProfessionalService service)
    : IRequestHandler<GetProfessionalServicesQuery, IEnumerable<object>>
{
    public async Task<IEnumerable<object>> Handle(GetProfessionalServicesQuery request, CancellationToken ct)
    {
        return await service.GetProfessionalServicesAsync(request.ProfileId, ct);
    }
}
