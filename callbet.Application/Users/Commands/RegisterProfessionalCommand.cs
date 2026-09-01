using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Users.Commands;

public record RegisterProfessionalCommand(UserDto Dto) : IRequest<Guid>;
