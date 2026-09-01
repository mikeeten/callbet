using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Professionals.Commands;

public class RefreshReputationHandler(IProfessionalService service)
    : IRequestHandler<RefreshReputationCommand, bool>
{
    public async Task<bool> Handle(RefreshReputationCommand request, CancellationToken ct)
    {
        return await service.RefreshReputationAsync(request.UserId, ct);
    }
}
