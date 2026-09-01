using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Jobs.Commands;

public class HoldPaymentHandler(IJobService service)
    : IRequestHandler<HoldPaymentCommand, Guid>
{
    public async Task<Guid> Handle(HoldPaymentCommand request, CancellationToken ct)
    {
        return await service.HoldPaymentAsync(request.JobId, ct);
    }
}