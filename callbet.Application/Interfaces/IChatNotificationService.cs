using callbet.Application.DTOs;

namespace callbet.Application.Interfaces;

public interface IChatNotificationService
{
    Task<Guid> CreateChatSessionAsync(ChatSessionDto dto, CancellationToken ct);
    Task<Guid> SendMessageAsync(ChatMessageDto dto, CancellationToken ct);
    Task<IEnumerable<ChatMessageDto>> GetMessagesAsync(Guid sessionId, CancellationToken ct);
    Task<IEnumerable<ChatSessionDto>> GetUserSessionsAsync(Guid userId, CancellationToken ct);
    Task<bool> MarkNotificationAsReadAsync(Guid notificationId, Guid userId, CancellationToken ct);
    Task<bool> MarkMessageAsReadAsync(Guid messageId, Guid userId, CancellationToken ct);
    Task<bool> DeleteMessageAsync(Guid messageId, Guid adminUserId = default, CancellationToken ct = default);
    Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(Guid userId, CancellationToken ct);
}
