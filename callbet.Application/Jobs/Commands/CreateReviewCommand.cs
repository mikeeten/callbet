using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Jobs.Commands;

public record CreateReviewCommand(ReviewDto Dto) : IRequest<Guid>;