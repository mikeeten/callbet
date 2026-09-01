using MediatR;

namespace callbet.Application.Jobs.Commands;

public record StartJobCommand(Guid JobId) : IRequest<Guid>;