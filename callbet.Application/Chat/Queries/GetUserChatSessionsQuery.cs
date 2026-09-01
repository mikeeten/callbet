using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Chat.Queries;

public record GetUserChatSessionsQuery(Guid UserId) : IRequest<IEnumerable<ChatSessionDto>>;
