using MediatR;

namespace callbet.Application.Admin.Commands;

public record ApproveProfessionalCommand(Guid UserId) : IRequest<Guid>;