using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Chat.Commands;

public class MarkMessageAsReadHandler(IChatNotificationService service)
    : IRequestHandler<MarkMessageAsReadCommand, bool>
{
    public async Task<bool> Handle(MarkMessageAsReadCommand request, CancellationToken ct)
    {
        return await service.MarkMessageAsReadAsync(request.MessageId, request.UserId, ct);
    }
}
