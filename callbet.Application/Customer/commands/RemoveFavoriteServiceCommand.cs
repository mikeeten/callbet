using MediatR;

namespace callbet.Application.Customer.Commands;

public record RemoveFavoriteServiceCommand(Guid CustomerId, Guid ServiceId) : IRequest<bool>;
