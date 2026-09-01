using MediatR;
using callbet.Domain.Entities;

namespace callbet.Application.Admin.Queries;

public record GetServiceCategoriesQuery() : IRequest<IEnumerable<ServiceCategory>>;