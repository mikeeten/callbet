using MediatR;
using callbet.Application.Interfaces;
using callbet.Application.DTOs;

namespace callbet.Application.Customer.Queries;

public class GetUserAddressesHandler(ICustomerService service) : IRequestHandler<GetUserAddressesQuery, IEnumerable<AddressDto>>
{
    public async Task<IEnumerable<AddressDto>> Handle(GetUserAddressesQuery request, CancellationToken ct)
    {
        return await service.GetUserAddressesAsync(request.UserId, ct);
    }
}
