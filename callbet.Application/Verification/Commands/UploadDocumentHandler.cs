using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Verification.Commands;

public class UploadDocumentHandler(IVerificationService service)
    : IRequestHandler<UploadDocumentCommand, Guid>
{
    public async Task<Guid> Handle(UploadDocumentCommand request, CancellationToken ct)
    {
        return await service.UploadDocumentAsync(request.Dto, ct);
    }
}