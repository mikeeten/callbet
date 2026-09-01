using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Jobs.Commands;

public class ReplyToReviewHandler(IJobService service)
    : IRequestHandler<ReplyToReviewCommand, Guid>
{
    public async Task<Guid> Handle(ReplyToReviewCommand request, CancellationToken ct)
    {
        return await service.ReplyToReviewAsync(request.Dto, ct);
    }
}