using callbet.Application.DTOs;
using callbet.Domain.Entities;

namespace callbet.Application.Interfaces;

public interface IVerificationService
{
    Task<Guid> UploadDocumentAsync(VerificationRecordDto dto, CancellationToken ct);
    Task<PagedResponse<VerificationRecord>> GetVerificationRecordsAsync(PagedRequest request, CancellationToken ct);
    Task<Guid> ApproveVerificationAsync(Guid recordId, CancellationToken ct);
    Task<Guid> DeleteVerificationAsync(Guid recordId, CancellationToken ct);
}