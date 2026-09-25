using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Chat.Commands;

public class DeleteChatMessageHandler(IChatNotificationService service)
    : IRequestHandler<DeleteChatMessageCommand, bool>
{
    public async Task<bool> Handle(DeleteChatMessageCommand request, CancellationToken ct)
    {
        return await service.DeleteMessageAsync(request.MessageId, request.AdminUserId, ct);
    }
}
