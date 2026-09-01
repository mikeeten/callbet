using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Chat.Commands;

public class SendMessageHandler(IChatNotificationService service)
    : IRequestHandler<SendMessageCommand, Guid>
{
    public async Task<Guid> Handle(SendMessageCommand request, CancellationToken ct)
    {
        return await service.SendMessageAsync(request.Dto, ct);
    }
}
