using MediatR;

namespace callbet.Application.Chat.Commands;

public record MarkNotificationAsReadCommand(Guid NotificationId, Guid UserId) : IRequest<bool>;
