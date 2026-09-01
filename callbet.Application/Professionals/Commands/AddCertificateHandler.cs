using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Professionals.Commands;

public class AddCertificateHandler(IProfessionalService service)
    : IRequestHandler<AddCertificateCommand, Guid>
{
    public async Task<Guid> Handle(AddCertificateCommand request, CancellationToken ct)
    {
        return await service.AddCertificateAsync(request.Dto, ct);
    }
}
