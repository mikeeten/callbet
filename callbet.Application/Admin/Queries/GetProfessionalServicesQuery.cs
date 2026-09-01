using MediatR;

namespace callbet.Application.Admin.Queries;

public record GetProfessionalServicesQuery(Guid ProfileId) : IRequest<IEnumerable<object>>;