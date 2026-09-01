using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Customer.Commands;

public class AddFavoriteServiceHandler(ICustomerService service)
    : IRequestHandler<AddFavoriteServiceCommand, Guid>
{
    public async Task<Guid> Handle(AddFavoriteServiceCommand request, CancellationToken ct)
    {
        return await service.AddFavoriteServiceAsync(request.Dto, ct);
    }
}
