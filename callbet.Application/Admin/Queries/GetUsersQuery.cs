using callbet.Application.DTOs;
using callbet.Domain.Entities;
using MediatR;

namespace callbet.Application.Admin.Queries;

public record GetUsersQuery(PagedRequest Request) : IRequest<PagedResponse<UserResponseDto>>;