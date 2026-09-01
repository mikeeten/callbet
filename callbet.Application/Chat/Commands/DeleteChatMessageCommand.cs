using MediatR;

namespace callbet.Application.Chat.Commands;

public record DeleteChatMessageCommand(Guid MessageId) : IRequest<bool>;
