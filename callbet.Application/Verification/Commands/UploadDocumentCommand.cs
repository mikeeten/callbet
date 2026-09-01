using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Verification.Commands;

public record UploadDocumentCommand(VerificationRecordDto Dto) : IRequest<Guid>;