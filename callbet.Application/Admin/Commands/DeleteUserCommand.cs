using MediatR;

namespace callbet.Application.Admin.Commands;

public record DeleteUserCommand(Guid UserId) : IRequest<Guid>;