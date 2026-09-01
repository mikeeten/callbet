using MediatR;

namespace callbet.Application.Customer.Commands;

public record RemoveFavoriteProfessionalCommand(Guid CustomerId, Guid ProfessionalProfileId) : IRequest<bool>;
