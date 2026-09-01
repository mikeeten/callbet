using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Verification.Commands;

public class DeleteVerificationHandler(IVerificationService service)
    : IRequestHandler<DeleteVerificationCommand, Guid>
{
    public async Task<Guid> Handle(DeleteVerificationCommand request, CancellationToken ct)
    {
        return await service.DeleteVerificationAsync(request.RecordId, ct);
    }
}