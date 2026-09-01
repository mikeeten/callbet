using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Professionals.Commands;

public record AddPortfolioItemCommand(PortfolioItemDto Dto) : IRequest<Guid>;
