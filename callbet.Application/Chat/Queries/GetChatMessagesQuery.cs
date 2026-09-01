using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Chat.Queries;

public record GetChatMessagesQuery(Guid SessionId) : IRequest<IEnumerable<ChatMessageDto>>;
