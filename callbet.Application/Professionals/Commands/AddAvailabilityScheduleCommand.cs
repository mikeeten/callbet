using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Professionals.Commands;

public record AddAvailabilityScheduleCommand(AvailabilityScheduleDto Dto) : IRequest<Guid>;
