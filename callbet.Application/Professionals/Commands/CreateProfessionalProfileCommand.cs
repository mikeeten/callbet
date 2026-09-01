using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Professionals.Commands;

public record CreateProfessionalProfileCommand(ProfessionalProfileDto Dto) : IRequest<Guid>;
