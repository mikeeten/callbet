using MediatR;
using callbet.Domain.Entities;

namespace callbet.Application.Admin.Queries;

public record GetServicesByCategoryQuery(Guid CategoryId) : IRequest<IEnumerable<Service>>;