using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Professionals.Queries;

public record GetProfessionalProfileDashboardQuery(Guid Id) : IRequest<GetProfessionalProfileDashboardDto?>;
