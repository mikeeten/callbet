using MediatR;

namespace callbet.Application.Professionals.Commands;

public record RefreshReputationCommand(Guid UserId) : IRequest<bool>;
