using MediatR;

namespace callbet.Application.Customer.Queries;

public record GetFavoriteProfessionalsQuery(Guid CustomerId) : IRequest<IEnumerable<object>>;
