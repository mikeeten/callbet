using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Chat.Commands;

public record CreateChatSessionCommand(ChatSessionDto Dto) : IRequest<Guid>;
