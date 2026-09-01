using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Customer.Commands;

public class RemoveFavoriteProfessionalHandler(ICustomerService service)
    : IRequestHandler<RemoveFavoriteProfessionalCommand, bool>
{
    public async Task<bool> Handle(RemoveFavoriteProfessionalCommand request, CancellationToken ct)
    {
        return await service.RemoveFavoriteProfessionalAsync(request.CustomerId, request.ProfessionalProfileId, ct);
    }
}
