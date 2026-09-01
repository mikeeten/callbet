using MediatR;

namespace callbet.Application.Customer.Queries;

public record GetFavoriteServicesQuery(Guid CustomerId) : IRequest<IEnumerable<object>>;
