using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Users.Commands;

public class RegisterCustomerHandler(ICustomerService service)
    : IRequestHandler<RegisterCustomerCommand, Guid>
{
    public async Task<Guid> Handle(RegisterCustomerCommand request, CancellationToken ct)
    {
        // Delegate to the service implementation in Infrastructure
        return await service.RegisterCustomerAsync(request.Dto, ct);
    }
}
