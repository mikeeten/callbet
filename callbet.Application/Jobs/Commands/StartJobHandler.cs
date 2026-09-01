using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Jobs.Commands;

public class StartJobHandler(IJobService service)
    : IRequestHandler<StartJobCommand, Guid>
{
    public async Task<Guid> Handle(StartJobCommand request, CancellationToken ct)
    {
        return await service.StartJobAsync(request.JobId, ct);
    }
}