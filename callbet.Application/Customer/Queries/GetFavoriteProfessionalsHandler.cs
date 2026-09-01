using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Customer.Queries;

public class GetFavoriteProfessionalsHandler(ICustomerService service)
    : IRequestHandler<GetFavoriteProfessionalsQuery, IEnumerable<object>>
{
    public async Task<IEnumerable<object>> Handle(GetFavoriteProfessionalsQuery request, CancellationToken ct)
    {
        return await service.GetFavoriteProfessionalsAsync(request.CustomerId, ct);
    }
}
