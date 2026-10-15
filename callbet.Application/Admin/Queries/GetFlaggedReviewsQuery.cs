using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Admin.Queries;

public record GetFlaggedReviewsQuery(PagedRequest Request) : IRequest<PagedResponse<FlaggedReviewAdminDto>>;
