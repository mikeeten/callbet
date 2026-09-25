using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Admin.Queries;

public record GetServiceZonesQuery() : IRequest<IEnumerable<ServiceZoneDto>>;
