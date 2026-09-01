using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Professionals.Commands;

public record AssignServiceCommand(ProfessionalServiceDto Dto) : IRequest<Guid>;
