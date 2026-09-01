using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Jobs.Commands;

public class CreatePaymentHandler(IJobService service)
    : IRequestHandler<CreatePaymentCommand, Guid>
{
    public async Task<Guid> Handle(CreatePaymentCommand request, CancellationToken ct)
    {
        return await service.CreatePaymentAsync(request.Dto, ct);
    }
}