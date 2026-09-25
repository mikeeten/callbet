using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Jobs.Commands;

public class DeleteReviewHandler(IJobService jobService)
    : IRequestHandler<DeleteReviewCommand, bool>
{
    public async Task<bool> Handle(DeleteReviewCommand request, CancellationToken ct)
    {
        return await jobService.DeleteReviewAsync(request.ReviewId, request.AdminUserId, ct);
    }
}
