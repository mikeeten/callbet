using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Professionals.Commands;

public class UploadResumeHandler(IProfessionalService service)
    : IRequestHandler<UploadResumeCommand, Guid>
{
    public async Task<Guid> Handle(UploadResumeCommand request, CancellationToken ct)
    {
        return await service.UploadResumeAsync(request.Dto, ct);
    }
}
