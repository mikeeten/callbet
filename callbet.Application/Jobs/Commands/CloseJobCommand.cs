using MediatR;

namespace callbet.Application.Jobs.Commands;

public record CloseJobCommand(Guid JobId) : IRequest<Guid>;