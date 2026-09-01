using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Jobs.Commands;

public class CloseJobHandler(IJobService service)
    : IRequestHandler<CloseJobCommand, Guid>
{
    public async Task<Guid> Handle(CloseJobCommand request, CancellationToken ct)
    {
        return await service.CloseJobAsync(request.JobId, ct);
    }
}