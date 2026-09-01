using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Customer.Commands;

public record AddAddressCommand(AddressDto Dto) : IRequest<Guid>;
