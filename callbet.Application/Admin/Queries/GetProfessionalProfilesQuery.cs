using callbet.Application.DTOs;
using MediatR;

namespace callbet.Application.Admin.Queries;

public record GetProfessionalProfilesQuery(PagedRequest Request) : IRequest<PagedResponse<ProfessionalProfileAdminDto>>;
