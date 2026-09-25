using System;
using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Admin.Commands;

public record CreateServiceZoneCommand(CreateServiceZoneDto Dto, Guid AdminUserId) : IRequest<ServiceZoneDto>;
