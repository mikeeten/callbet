using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Jobs.Commands;

public class CreateReviewHandler(IJobService service)
    : IRequestHandler<CreateReviewCommand, Guid>
{
    public async Task<Guid> Handle(CreateReviewCommand request, CancellationToken ct)
    {
        return await service.CreateReviewAsync(request.Dto, ct);
    }
}