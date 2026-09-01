using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Professionals.Commands;

public class AddAvailabilityScheduleHandler(IProfessionalService service)
    : IRequestHandler<AddAvailabilityScheduleCommand, Guid>
{
    public async Task<Guid> Handle(AddAvailabilityScheduleCommand request, CancellationToken ct)
    {
        return await service.AddAvailabilityScheduleAsync(request.Dto, ct);
    }
}
