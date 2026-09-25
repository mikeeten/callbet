using MediatR;

namespace callbet.Application.Admin.Commands;

public record ApproveProfessionalCommand(Guid UserId, Guid AdminUserId = default) : IRequest<Guid>;