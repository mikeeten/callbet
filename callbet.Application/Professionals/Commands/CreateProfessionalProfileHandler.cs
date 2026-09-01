using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Professionals.Commands;

public class CreateProfessionalProfileHandler(IProfessionalService service)
    : IRequestHandler<CreateProfessionalProfileCommand, Guid>
{
    public async Task<Guid> Handle(CreateProfessionalProfileCommand request, CancellationToken ct)
    {
        return await service.CreateProfileAsync(request.Dto, ct);
    }
}
