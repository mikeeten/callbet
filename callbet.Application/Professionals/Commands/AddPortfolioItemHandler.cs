using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Professionals.Commands;

public class AddPortfolioItemHandler(IProfessionalService service)
    : IRequestHandler<AddPortfolioItemCommand, Guid>
{
    public async Task<Guid> Handle(AddPortfolioItemCommand request, CancellationToken ct)
    {
        return await service.AddPortfolioItemAsync(request.Dto, ct);
    }
}
