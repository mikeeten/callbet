using MediatR;

namespace callbet.Application.Admin.Commands;

public record ActivateUserCommand(Guid UserId, Guid AdminUserId = default) : IRequest<Guid>;
