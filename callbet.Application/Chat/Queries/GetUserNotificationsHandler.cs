using MediatR;
using callbet.Application.DTOs;
using callbet.Application.Interfaces;

namespace callbet.Application.Chat.Queries;

public class GetUserNotificationsHandler(IChatNotificationService service)
    : IRequestHandler<GetUserNotificationsQuery, IEnumerable<NotificationDto>>
{
    public async Task<IEnumerable<NotificationDto>> Handle(GetUserNotificationsQuery request, CancellationToken ct)
    {
        return await service.GetUserNotificationsAsync(request.UserId, ct);
    }
}
