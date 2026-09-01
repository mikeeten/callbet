using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Admin.Commands;

public record AssignServiceCommand(ProfessionalServiceDto Dto) : IRequest<Guid>;