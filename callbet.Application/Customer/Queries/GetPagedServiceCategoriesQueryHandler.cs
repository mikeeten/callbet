using MediatR;
using callbet.Domain.Entities;
using callbet.Application.Interfaces;
using callbet.Application.DTOs;

namespace callbet.Application.Customer.Queries;

public class GetPagedServiceCategoriesQueryHandler(ICustomerService service) : IRequestHandler<GetPagedServiceCategoriesQuery, PagedResponse<ServiceCategory>>
{
    private readonly ICustomerService _service = service;

    public async Task<PagedResponse<ServiceCategory>> Handle(GetPagedServiceCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetServiceCategoriesPagedAsync(request.Request, cancellationToken);
    }
}