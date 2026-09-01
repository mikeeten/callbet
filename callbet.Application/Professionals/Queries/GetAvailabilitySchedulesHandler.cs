using MediatR;
using callbet.Application.DTOs;
using callbet.Application.Interfaces;

namespace callbet.Application.Professionals.Queries;

public class GetAvailabilitySchedulesHandler(IProfessionalService service)
    : IRequestHandler<GetAvailabilitySchedulesQuery, IEnumerable<AvailabilityScheduleDto>>
{
    public async Task<IEnumerable<AvailabilityScheduleDto>> Handle(GetAvailabilitySchedulesQuery request, CancellationToken ct)
    {
        return await service.GetAvailabilitySchedulesAsync(request.ProfileId, ct);
    }
}
