using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Chat.Commands;

public record SendMessageCommand(ChatMessageDto Dto) : IRequest<Guid>;
