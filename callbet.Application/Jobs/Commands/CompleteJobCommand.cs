using MediatR;

namespace callbet.Application.Jobs.Commands;

public record CompleteJobCommand(Guid JobId) : IRequest<Guid>;