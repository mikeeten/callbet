using MediatR;

namespace callbet.Application.Verification.Commands;

public record ApproveVerificationCommand(Guid RecordId) : IRequest<Guid>;