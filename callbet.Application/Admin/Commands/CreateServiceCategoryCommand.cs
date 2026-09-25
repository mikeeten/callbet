using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Admin.Commands;

public record CreateServiceCategoryCommand(ServiceCategoryDto Dto, Guid AdminUserId = default) : IRequest<Guid>;