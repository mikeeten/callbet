using MediatR;

namespace callbet.Application.Jobs.Queries;

public record GetJobsForProfessionalQuery(Guid ProfessionalId) : IRequest<IEnumerable<object>>;
