using callbet.Application.Interfaces;
using callbet.Domain.Entities;
using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Verification.Queries;

public class GetVerificationRecordsHandler(IVerificationService service)
    : IRequestHandler<GetVerificationRecordsQuery, PagedResponse<VerificationRecordDto>>
{
    public async Task<PagedResponse<VerificationRecordDto>> Handle(GetVerificationRecordsQuery request, CancellationToken ct)
    {
        return await service.GetVerificationRecordsAsync(request.Request, ct);
    }
}
