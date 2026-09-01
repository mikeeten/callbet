using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Customer.Commands;

public record AddFavoriteProfessionalCommand(FavoriteProfessionalDto Dto) : IRequest<Guid>;
