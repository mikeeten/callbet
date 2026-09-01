using MediatR;

namespace callbet.Application.Chat.Commands;

public record MarkMessageAsReadCommand(Guid MessageId, Guid UserId) : IRequest<bool>;
