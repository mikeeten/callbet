using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Customer.Commands;

public class AddAddressHandler(ICustomerService service) : IRequestHandler<AddAddressCommand, Guid>
{
    public async Task<Guid> Handle(AddAddressCommand request, CancellationToken ct)
    {
        return await service.AddAddressAsync(request.Dto, ct);
    }
}
