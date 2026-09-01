using MediatR;

namespace callbet.Application.Jobs.Commands;

public record ReleasePaymentCommand(Guid JobId) : IRequest<Guid>;