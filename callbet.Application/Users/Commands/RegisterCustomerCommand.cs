using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Users.Commands;

public record RegisterCustomerCommand(UserDto Dto) : IRequest<Guid>;
