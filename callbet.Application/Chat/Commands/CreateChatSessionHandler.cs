using MediatR;
using callbet.Application.Interfaces;

namespace callbet.Application.Chat.Commands;

public class CreateChatSessionHandler(IChatNotificationService service)
    : IRequestHandler<CreateChatSessionCommand, Guid>
{
    public async Task<Guid> Handle(CreateChatSessionCommand request, CancellationToken ct)
    {
        return await service.CreateChatSessionAsync(request.Dto, ct);
    }
}
