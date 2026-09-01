using MediatR;

namespace callbet.Application.Verification.Queries;

public record GetPendingVerificationsQuery() : IRequest<IEnumerable<object>>;