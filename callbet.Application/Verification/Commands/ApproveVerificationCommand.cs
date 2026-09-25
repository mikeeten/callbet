using MediatR;

namespace callbet.Application.Verification.Commands;

public record ApproveVerificationCommand(Guid RecordId, Guid AdminUserId = default) : IRequest<Guid>;