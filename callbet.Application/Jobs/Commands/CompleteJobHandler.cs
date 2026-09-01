using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Jobs.Commands;

public class CompleteJobHandler(IJobService service)
    : IRequestHandler<CompleteJobCommand, Guid>
{
    public async Task<Guid> Handle(CompleteJobCommand request, CancellationToken ct)
    {
        return await service.CompleteJobAsync(request.JobId, ct);
    }
}