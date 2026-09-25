using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Admin.Commands;

public record CreateServiceCommand(ServiceDto Dto, Guid AdminUserId = default) : IRequest<Guid>;