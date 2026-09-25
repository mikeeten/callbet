using MediatR;

namespace callbet.Application.Admin.Commands;

public record SuspendProfessionalCommand(Guid UserId, Guid AdminUserId = default) : IRequest<Guid>;