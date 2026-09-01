using MediatR;

namespace callbet.Application.Admin.Commands;

public record SuspendProfessionalCommand(Guid UserId) : IRequest<Guid>;