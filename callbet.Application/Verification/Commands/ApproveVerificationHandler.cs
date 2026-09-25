using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Verification.Commands;

public class ApproveVerificationHandler(IVerificationService service)
    : IRequestHandler<ApproveVerificationCommand, Guid>
{
    public async Task<Guid> Handle(ApproveVerificationCommand request, CancellationToken ct)
    {
        return await service.ApproveVerificationAsync(request.RecordId, request.AdminUserId, ct);
    }
}