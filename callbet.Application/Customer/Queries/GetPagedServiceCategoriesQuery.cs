using MediatR;
using callbet.Domain.Entities;
using callbet.Application.Interfaces;
using callbet.Application.DTOs;

namespace callbet.Application.Customer.Queries;

public record GetPagedServiceCategoriesQuery(PagedRequest Request) : IRequest<PagedResponse<ServiceCategory>>;