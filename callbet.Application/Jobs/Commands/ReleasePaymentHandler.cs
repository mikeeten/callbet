using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Jobs.Commands;

public class ReleasePaymentHandler(IJobService service)
    : IRequestHandler<ReleasePaymentCommand, Guid>
{
    public async Task<Guid> Handle(ReleasePaymentCommand request, CancellationToken ct)
    {
        return await service.ReleasePaymentAsync(request.JobId, ct);
    }
}