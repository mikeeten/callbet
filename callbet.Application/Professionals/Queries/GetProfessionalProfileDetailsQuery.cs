using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Professionals.Queries;

public record GetProfessionalProfileDetailsQuery(Guid Id) : IRequest<ProfessionalProfileDetailsDto?>;
