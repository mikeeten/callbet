using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Customer.Queries;

public class GetFavoriteServicesHandler(ICustomerService service)
    : IRequestHandler<GetFavoriteServicesQuery, IEnumerable<object>>
{
    public async Task<IEnumerable<object>> Handle(GetFavoriteServicesQuery request, CancellationToken ct)
    {
        return await service.GetFavoriteServicesAsync(request.CustomerId, ct);
    }
}
