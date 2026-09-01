using MediatR;
using callbet.Application.DTOs;
using callbet.Application.Interfaces;

namespace callbet.Application.Chat.Queries;

public class GetUserChatSessionsHandler(IChatNotificationService service)
    : IRequestHandler<GetUserChatSessionsQuery, IEnumerable<ChatSessionDto>>
{
    public async Task<IEnumerable<ChatSessionDto>> Handle(GetUserChatSessionsQuery request, CancellationToken ct)
    {
        return await service.GetUserSessionsAsync(request.UserId, ct);
    }
}
