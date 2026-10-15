using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Chat.Queries;

public record GetUserNotificationsQuery(Guid UserId, PagedRequest Request) : IRequest<PagedResponse<NotificationDto>>;
