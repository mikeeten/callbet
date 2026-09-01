using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Jobs.Queries;

public record GetReviewByJobIdQuery(Guid JobId) : IRequest<ReviewDto?>;
