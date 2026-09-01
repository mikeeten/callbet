using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Customer.Commands;

public class AddFavoriteProfessionalHandler(ICustomerService service)
    : IRequestHandler<AddFavoriteProfessionalCommand, Guid>
{
    public async Task<Guid> Handle(AddFavoriteProfessionalCommand request, CancellationToken ct)
    {
        return await service.AddFavoriteProfessionalAsync(request.Dto, ct);
    }
}
