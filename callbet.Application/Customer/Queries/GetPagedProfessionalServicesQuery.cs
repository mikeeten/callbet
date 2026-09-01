using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Customer.Queries;

public record GetPagedProfessionalServicesQuery(PagedRequest Request) 
    : IRequest<PagedResponse<ProfessionalProfileServiceDto>>;
