using MediatR;

namespace callbet.Application.Jobs.Commands;

public record DeleteReviewCommand(Guid ReviewId, Guid AdminUserId = default) : IRequest<bool>;
