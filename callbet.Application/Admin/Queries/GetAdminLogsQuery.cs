using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Admin.Queries;

public record GetAdminLogsQuery(PagedRequest Request) : IRequest<PagedResponse<AdminLogDto>>;
