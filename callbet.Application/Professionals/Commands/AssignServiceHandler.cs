using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Professionals.Commands;

public class AssignServiceHandler(IProfessionalService service)
    : IRequestHandler<AssignServiceCommand, Guid>
{
    public async Task<Guid> Handle(AssignServiceCommand request, CancellationToken ct)
    {
        return await service.AssignServiceAsync(request.Dto, ct);
    }
}
