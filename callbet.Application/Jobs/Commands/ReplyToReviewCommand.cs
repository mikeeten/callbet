using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Jobs.Commands;

public record ReplyToReviewCommand(ReviewReplyDto Dto) : IRequest<Guid>;