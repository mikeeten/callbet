using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Chat.Commands;

public class MarkNotificationAsReadHandler(IChatNotificationService service)
    : IRequestHandler<MarkNotificationAsReadCommand, bool>
{
    public async Task<bool> Handle(MarkNotificationAsReadCommand request, CancellationToken ct)
    {
        return await service.MarkNotificationAsReadAsync(request.NotificationId, request.UserId, ct);
    }
}
