using MediatR;

namespace callbet.Application.Verification.Commands;

public record DeleteVerificationCommand(Guid RecordId) : IRequest<Guid>;