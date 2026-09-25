using MediatR;

namespace callbet.Application.Chat.Commands;

public record DeleteChatMessageCommand(Guid MessageId, Guid AdminUserId = default) : IRequest<bool>;
