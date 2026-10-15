using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Admin.Queries;

public record GetReportsQuery(PagedRequest Request) : IRequest<PagedResponse<AdminReportDto>>;
