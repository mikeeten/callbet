using MediatR;

namespace callbet.Application.Admin.Queries;

public record GetPendingProfessionalsQuery() : IRequest<IEnumerable<object>>;