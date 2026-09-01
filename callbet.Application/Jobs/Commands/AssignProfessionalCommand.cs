using MediatR;

namespace callbet.Application.Jobs.Commands;

public record AssignProfessionalCommand(Guid JobId, Guid ProfessionalId) : IRequest<Guid>;