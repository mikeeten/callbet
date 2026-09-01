using MediatR;

namespace callbet.Application.Jobs.Commands;

public record HoldPaymentCommand(Guid JobId) : IRequest<Guid>;