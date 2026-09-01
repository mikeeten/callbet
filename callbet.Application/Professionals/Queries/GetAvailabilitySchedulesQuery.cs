using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Professionals.Queries;

public record GetAvailabilitySchedulesQuery(Guid ProfileId) : IRequest<IEnumerable<AvailabilityScheduleDto>>;
