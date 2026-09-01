using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Users.Commands;

public record LoginCommand(string Email, string Password) : IRequest<UserDto?>;
