using MediatR;

namespace callbet.Application.Jobs.Commands;

public record DeleteReviewCommand(Guid ReviewId) : IRequest<bool>;
