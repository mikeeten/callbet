using callbet.Application.Interfaces;
using callbet.Domain.Entities;
using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Verification.Queries;

public class GetVerificationRecordsHandler(IVerificationService service)
    : IRequestHandler<GetVerificationRecordsQuery, PagedResponse<VerificationRecord>>
{
    public async Task<PagedResponse<VerificationRecord>> Handle(GetVerificationRecordsQuery request, CancellationToken ct)
    {
        return await service.GetVerificationRecordsAsync(request.Request, ct);
    }
}
