using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Jobs.Queries;

public record GetReviewsForProfessionalQuery(Guid ProfessionalId) : IRequest<IEnumerable<ReviewDto>>;
