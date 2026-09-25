using System;
using MediatR;

namespace callbet.Application.Admin.Commands;

public record DeleteServiceZoneCommand(int NeighborhoodId, Guid AdminUserId) : IRequest<bool>;
