using MediatR;
using callbet.Application.DTOs;
using callbet.Application.Interfaces;

namespace callbet.Application.Chat.Queries;

public class GetChatMessagesHandler(IChatNotificationService service)
    : IRequestHandler<GetChatMessagesQuery, IEnumerable<ChatMessageDto>>
{
    public async Task<IEnumerable<ChatMessageDto>> Handle(GetChatMessagesQuery request, CancellationToken ct)
    {
        return await service.GetMessagesAsync(request.SessionId, ct);
    }
}
