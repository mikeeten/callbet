using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Admin.Commands;

public record RegisterAdminCommand(UserDto Dto) : IRequest<Guid>;