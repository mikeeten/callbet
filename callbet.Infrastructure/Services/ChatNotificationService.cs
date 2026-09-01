using callbet.Application.DTOs;
using callbet.Application.Interfaces;
using callbet.Domain.Entities;
using callbet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace callbet.Infrastructure.Services;

public class ChatNotificationService(CallbetDbContext context) : IChatNotificationService
{
    public async Task<Guid> CreateChatSessionAsync(ChatSessionDto dto, CancellationToken ct)
    {
        var existing = await context.ChatSessions
            .FirstOrDefaultAsync(cs => (cs.CustomerId == dto.CustomerId && cs.ProfessionalId == dto.ProfessionalId) ||
                                       (cs.CustomerId == dto.ProfessionalId && cs.ProfessionalId == dto.CustomerId), ct);
        if (existing != null) return existing.Id;

        var session = new ChatSession
        {
            Id = dto.Id != Guid.Empty ? dto.Id : Guid.NewGuid(),
            CustomerId = dto.CustomerId,
            ProfessionalId = dto.ProfessionalId,
            CreatedAt = DateTime.UtcNow
        };

        context.ChatSessions.Add(session);
        await context.SaveChangesAsync(ct);
        return session.Id;
    }

    public async Task<Guid> SendMessageAsync(ChatMessageDto dto, CancellationToken ct)
    {
        var session = await context.ChatSessions.FindAsync(new object[] { dto.ChatSessionId }, ct);

        var message = new ChatMessage
        {
            Id = dto.Id != Guid.Empty ? dto.Id : Guid.NewGuid(),
            ChatSessionId = dto.ChatSessionId,
            SenderId = dto.SenderId,
            Content = dto.Content,
            SentAt = DateTime.UtcNow
        };

        context.ChatMessages.Add(message);

        // Determine recipient for notification
        var receiverId = dto.ReceiverId != Guid.Empty
            ? dto.ReceiverId
            : (session != null ? (session.CustomerId == dto.SenderId ? session.ProfessionalId : session.CustomerId) : Guid.Empty);

        if (receiverId != Guid.Empty)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = receiverId,
                Message = "You have received a new message.",
                Link = $"/chat/{dto.ChatSessionId}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            context.Notifications.Add(notification);
        }

        await context.SaveChangesAsync(ct);
        return message.Id;
    }

    public async Task<IEnumerable<ChatMessageDto>> GetMessagesAsync(Guid sessionId, CancellationToken ct)
    {
        return await context.ChatMessages
            .Where(cm => cm.ChatSessionId == sessionId)
            .OrderBy(cm => cm.SentAt)
            .Select(cm => new ChatMessageDto
            {
                Id = cm.Id,
                ChatSessionId = cm.ChatSessionId,
                SenderId = cm.SenderId,
                Content = cm.Content,
                SentAt = cm.SentAt
            })
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ChatSessionDto>> GetUserSessionsAsync(Guid userId, CancellationToken ct)
    {
        return await context.ChatSessions
            .Where(cs => cs.CustomerId == userId || cs.ProfessionalId == userId)
            .OrderByDescending(cs => cs.CreatedAt)
            .Select(cs => new ChatSessionDto
            {
                Id = cs.Id,
                CustomerId = cs.CustomerId,
                ProfessionalId = cs.ProfessionalId,
                CreatedAt = cs.CreatedAt
            })
            .ToListAsync(ct);
    }

    public async Task<bool> MarkNotificationAsReadAsync(Guid notificationId, Guid userId, CancellationToken ct)
    {
        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && (userId == Guid.Empty || n.UserId == userId), ct);

        if (notification == null) return false;

        notification.IsRead = true;
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> MarkMessageAsReadAsync(Guid messageId, Guid userId, CancellationToken ct)
    {
        var message = await context.ChatMessages.FindAsync(new object[] { messageId }, ct);
        if (message == null) return false;

        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteMessageAsync(Guid messageId, CancellationToken ct)
    {
        var message = await context.ChatMessages.FindAsync(new object[] { messageId }, ct);
        if (message == null) return false;

        context.ChatMessages.Remove(message);

        // 📝 INSERT INTO admin_logs
        context.AdminLogs.Add(new AdminLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = Guid.Empty,
            Action = "Removed abusive chat message",
            TargetEntity = "ChatMessage",
            TargetEntityId = messageId,
            Timestamp = DateTime.UtcNow
        });

        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(Guid userId, CancellationToken ct)
    {
        return await context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                UserId = n.UserId,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                Link = n.Link
            })
            .ToListAsync(ct);
    }
}
