using MediatR;

namespace callbet.Application.Jobs.Queries;

public record GetPaymentsForProfessionalQuery(Guid ProfessionalId) : IRequest<IEnumerable<object>>;
