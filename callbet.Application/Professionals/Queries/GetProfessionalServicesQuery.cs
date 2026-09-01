using MediatR;

namespace callbet.Application.Professionals.Queries;

public record GetProfessionalServicesQuery(Guid ProfileId) : IRequest<IEnumerable<object>>;
