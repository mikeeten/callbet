using MediatR;

namespace callbet.Application.Verification.Commands;

public record DeleteVerificationCommand(Guid RecordId, Guid AdminUserId = default) : IRequest<Guid>;