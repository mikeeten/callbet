using callbet.Application.DTOs;
using callbet.Application.Interfaces;
using callbet.Domain.Entities;
using callbet.Infrastructure.Hubs;
using callbet.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace callbet.Infrastructure.Services;

public class ChatNotificationService(
    CallbetDbContext context,
    IHubContext<ChatHub, IChatHubClient> hubContext) : IChatNotificationService
{
    public async Task<Guid> CreateChatSessionAsync(ChatSessionDto dto, CancellationToken ct)
    {
        // Resolve ProfessionalId to AspNetUsers.Id if a ProfessionalProfile.Id was provided
        var proProfile = await context.ProfessionalProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == dto.ProfessionalId, ct);
        var resolvedProUserId = proProfile != null ? proProfile.UserId : dto.ProfessionalId;

        var existing = await context.ChatSessions
            .FirstOrDefaultAsync(cs => (cs.CustomerId == dto.CustomerId && (cs.ProfessionalId == resolvedProUserId || cs.ProfessionalId == dto.ProfessionalId)) ||
                                       (cs.CustomerId == resolvedProUserId && cs.ProfessionalId == dto.CustomerId), ct);
        if (existing != null) return existing.Id;

        var session = new ChatSession
        {
            Id = dto.Id != Guid.Empty ? dto.Id : Guid.NewGuid(),
            CustomerId = dto.CustomerId,
            ProfessionalId = resolvedProUserId,
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

        Notification? notification = null;
        if (receiverId != Guid.Empty)
        {
            notification = new Notification
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

        // Resolve sender user info
        var senderUser = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == dto.SenderId, ct);
        var senderName = senderUser != null 
            ? $"{senderUser.FirstName} {senderUser.LastName}".Trim()
            : null;
        if (string.IsNullOrWhiteSpace(senderName) && senderUser != null)
        {
            senderName = senderUser.UserName;
        }

        // 📡 Real-time SignalR broadcasts after successful database commit
        var messageDto = new ChatMessageDto
        {
            Id = message.Id,
            ChatSessionId = message.ChatSessionId,
            SenderId = message.SenderId,
            SenderName = senderName,
            SenderRole = senderUser?.Role.ToString(),
            ReceiverId = receiverId,
            Content = message.Content,
            IsRead = false,
            SentAt = message.SentAt
        };

        // Broadcast to chat room session group
        await hubContext.Clients.Group(dto.ChatSessionId.ToString()).ReceiveChatMessage(messageDto);

        // Broadcast notification to recipient's direct channel
        if (notification != null)
        {
            var notificationDto = new NotificationDto
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Message = notification.Message,
                Link = notification.Link,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };

            await hubContext.Clients.Group($"user_{receiverId}").ReceiveNotification(notificationDto);
        }

        return message.Id;
    }

    public async Task<IEnumerable<ChatMessageDto>> GetMessagesAsync(Guid sessionId, CancellationToken ct)
    {
        var messages = await (from cm in context.ChatMessages.AsNoTracking()
                              where cm.ChatSessionId == sessionId
                              join user in context.Users.AsNoTracking() on cm.SenderId equals user.Id into userJoin
                              from senderUser in userJoin.DefaultIfEmpty()
                              orderby cm.SentAt
                              select new
                              {
                                  cm.Id,
                                  cm.ChatSessionId,
                                  cm.SenderId,
                                  cm.Content,
                                  cm.SentAt,
                                  FirstName = senderUser != null ? senderUser.FirstName : null,
                                  LastName = senderUser != null ? senderUser.LastName : null,
                                  Role = senderUser != null ? senderUser.Role.ToString() : null
                              }).ToListAsync(ct);

        return messages.Select(m => new ChatMessageDto
        {
            Id = m.Id,
            ChatSessionId = m.ChatSessionId,
            SenderId = m.SenderId,
            SenderName = !string.IsNullOrWhiteSpace(m.FirstName) || !string.IsNullOrWhiteSpace(m.LastName)
                ? $"{m.FirstName} {m.LastName}".Trim()
                : "Customer",
            SenderRole = m.Role,
            Content = m.Content,
            SentAt = m.SentAt
        });
    }

    public async Task<IEnumerable<ChatSessionDto>> GetUserSessionsAsync(Guid userId, CancellationToken ct)
    {
        // First find if this userId belongs to a professional profile
        var proProfile = await context.ProfessionalProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId || p.Id == userId, ct);
        var proId = proProfile?.Id ?? Guid.Empty;
        var proUserId = proProfile?.UserId ?? userId;

        var rawSessions = await context.ChatSessions
            .AsNoTracking()
            .Where(cs => cs.CustomerId == userId || cs.ProfessionalId == userId || (proId != Guid.Empty && cs.ProfessionalId == proId) || (proUserId != Guid.Empty && cs.ProfessionalId == proUserId))
            .OrderByDescending(cs => cs.CreatedAt)
            .ToListAsync(ct);

        var sessionIds = rawSessions.Select(s => s.Id).ToList();
        var allMessages = await context.ChatMessages
            .AsNoTracking()
            .Where(m => sessionIds.Contains(m.ChatSessionId))
            .OrderByDescending(m => m.SentAt)
            .ToListAsync(ct);

        var userIds = rawSessions.Select(s => s.CustomerId)
            .Concat(rawSessions.Select(s => s.ProfessionalId))
            .Distinct()
            .ToList();

        var users = await context.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, ct);

        var profiles = await context.ProfessionalProfiles
            .AsNoTracking()
            .Where(p => userIds.Contains(p.Id) || userIds.Contains(p.UserId))
            .ToListAsync(ct);

        var result = new List<ChatSessionDto>();

        foreach (var cs in rawSessions)
        {
            // Resolve Customer name
            string customerName = "Customer";
            string? customerAvatar = null;
            if (users.TryGetValue(cs.CustomerId, out var custUser))
            {
                var name = $"{custUser.FirstName} {custUser.LastName}".Trim();
                if (!string.IsNullOrWhiteSpace(name)) customerName = name;
                customerAvatar = custUser.ProfilePhotoUrl;
            }

            // Resolve Pro name
            string proName = "Professional";
            string? proAvatar = null;
            if (users.TryGetValue(cs.ProfessionalId, out var pUser))
            {
                var name = $"{pUser.FirstName} {pUser.LastName}".Trim();
                if (!string.IsNullOrWhiteSpace(name)) proName = name;
                proAvatar = pUser.ProfilePhotoUrl;
            }
            else
            {
                var prof = profiles.FirstOrDefault(p => p.Id == cs.ProfessionalId || p.UserId == cs.ProfessionalId);
                if (prof != null)
                {
                    if (!string.IsNullOrWhiteSpace(prof.Headline)) proName = prof.Headline;
                    proAvatar = null;
                }
            }

            var sessionMsgs = allMessages.Where(m => m.ChatSessionId == cs.Id).ToList();
            var latestMsg = sessionMsgs.FirstOrDefault();

            result.Add(new ChatSessionDto
            {
                Id = cs.Id,
                CustomerId = cs.CustomerId,
                CustomerName = customerName,
                CustomerAvatar = customerAvatar,
                ProfessionalId = cs.ProfessionalId,
                ProfessionalName = proName,
                ProfessionalAvatar = proAvatar,
                LastMessage = latestMsg?.Content ?? "No messages yet",
                LastMessageTime = latestMsg?.SentAt,
                CreatedAt = cs.CreatedAt
            });
        }

        return result;
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

        // Broadcast read status to the chat session group
        await hubContext.Clients.Group(message.ChatSessionId.ToString()).MessageReadStatus(messageId, userId);

        return true;
    }

    public async Task<bool> DeleteMessageAsync(Guid messageId, Guid adminUserId = default, CancellationToken ct = default)
    {
        var message = await context.ChatMessages.FindAsync(new object[] { messageId }, ct);
        if (message == null) return false;

        context.ChatMessages.Remove(message);

        // 📝 INSERT INTO admin_logs
        context.AdminLogs.Add(new AdminLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = adminUserId,
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
