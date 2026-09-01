using callbet.Application.DTOs;
using callbet.Domain.Entities;
using MediatR;

namespace callbet.Application.Verification.Queries;

public record GetVerificationRecordsQuery(PagedRequest Request) : IRequest<PagedResponse<VerificationRecord>>;