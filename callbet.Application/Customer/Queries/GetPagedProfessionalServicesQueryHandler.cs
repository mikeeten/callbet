using MediatR;
using callbet.Application.Interfaces;
using callbet.Application.DTOs;

namespace callbet.Application.Customer.Queries;

public class GetPagedProfessionalServicesQueryHandler(ICustomerService service) 
    : IRequestHandler<GetPagedProfessionalServicesQuery, PagedResponse<ProfessionalProfileServiceDto>>
{
    public async Task<PagedResponse<ProfessionalProfileServiceDto>> Handle(GetPagedProfessionalServicesQuery request, CancellationToken cancellationToken)
    {
        return await service.GetProfessionalServicesPagedAsync(request.Request, cancellationToken);
    }
}
