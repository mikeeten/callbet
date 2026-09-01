using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Jobs.Commands;

public class AssignProfessionalHandler(IJobService service)
    : IRequestHandler<AssignProfessionalCommand, Guid>
{
    public async Task<Guid> Handle(AssignProfessionalCommand request, CancellationToken ct)
    {
        return await service.AssignProfessionalAsync(request.JobId, request.ProfessionalId, ct);
    }
}