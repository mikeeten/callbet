using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Jobs.Commands;

public class CreateJobHandler(IJobService service)
    : IRequestHandler<CreateJobCommand, Guid>
{
    public async Task<Guid> Handle(CreateJobCommand request, CancellationToken ct)
    {
        return await service.CreateJobAsync(request.Dto, ct);
    }
}