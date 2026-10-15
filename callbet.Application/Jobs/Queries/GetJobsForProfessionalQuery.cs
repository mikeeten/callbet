using callbet.Application.DTOs;
using MediatR;

namespace callbet.Application.Jobs.Queries;

public record GetJobsForProfessionalQuery(Guid ProfessionalId, PagedRequest? Request = null) : IRequest<PagedResponse<ProfessionalJobDto>>;
