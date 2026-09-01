using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Customer.Commands;

public class RemoveFavoriteServiceHandler(ICustomerService service)
    : IRequestHandler<RemoveFavoriteServiceCommand, bool>
{
    public async Task<bool> Handle(RemoveFavoriteServiceCommand request, CancellationToken ct)
    {
        return await service.RemoveFavoriteServiceAsync(request.CustomerId, request.ServiceId, ct);
    }
}
