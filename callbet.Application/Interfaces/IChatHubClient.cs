using callbet.Application.DTOs;

namespace callbet.Application.Interfaces;

public interface IChatHubClient
{
    Task ReceiveChatMessage(ChatMessageDto message);
    Task ReceiveNotification(NotificationDto notification);
    Task MessageReadStatus(Guid messageId, Guid userId);
    Task UserTyping(Guid sessionId, Guid userId, bool isTyping);
}
