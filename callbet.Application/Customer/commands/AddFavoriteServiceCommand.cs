using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Customer.Commands;

public record AddFavoriteServiceCommand(FavoriteServiceDto Dto) : IRequest<Guid>;
