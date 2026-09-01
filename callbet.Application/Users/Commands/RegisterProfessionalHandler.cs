using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Users.Commands;

public class RegisterProfessionalHandler(ICustomerService service)
    : IRequestHandler<RegisterProfessionalCommand, Guid>
{
    public async Task<Guid> Handle(RegisterProfessionalCommand request, CancellationToken ct)
    {
        return await service.RegisterProfessionalAsync(request.Dto, ct);
    }
}
