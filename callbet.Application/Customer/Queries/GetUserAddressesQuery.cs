using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Customer.Queries;

public record GetUserAddressesQuery(Guid UserId) : IRequest<IEnumerable<AddressDto>>;
