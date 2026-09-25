using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using callbet.Application.DTOs;
using callbet.Application.Chat.Commands;
using callbet.Application.Chat.Queries;
using System.Security.Claims;

namespace callbet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/chat")]
public class ChatNotificationController(IMediator mediator) : ControllerBase
{
    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("id")?.Value
            ?? User.FindFirst("userId")?.Value
            ?? User.FindFirst(ClaimTypes.Sid)?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }

    // 1. Create or retrieve chat session
    [HttpPost("session")]
    public async Task<IActionResult> CreateChatSession([FromBody] ChatSessionDto dto)
    {
        var currentUserId = GetCurrentUserId();
        if (dto.CustomerId == Guid.Empty && currentUserId != Guid.Empty && dto.ProfessionalId != currentUserId)
        {
            dto.CustomerId = currentUserId;
        }
        else if (dto.ProfessionalId == Guid.Empty && currentUserId != Guid.Empty && dto.CustomerId != currentUserId)
        {
            dto.ProfessionalId = currentUserId;
        }

        if (dto.CustomerId == Guid.Empty || dto.ProfessionalId == Guid.Empty)
        {
            return BadRequest(new { Message = "Both CustomerId and ProfessionalId are required." });
        }

        var id = await mediator.Send(new CreateChatSessionCommand(dto));
        return Ok(new { Message = "Chat session created", Id = id, SessionId = id });
    }

    // 2. Send message and trigger notification
    [HttpPost("message")]
    public async Task<IActionResult> SendMessage([FromBody] ChatMessageDto dto)
    {
        if (dto.SenderId == Guid.Empty)
        {
            dto.SenderId = GetCurrentUserId();
        }

        if (dto.ChatSessionId == Guid.Empty)
        {
            return BadRequest(new { Message = "ChatSessionId is required." });
        }

        if (string.IsNullOrWhiteSpace(dto.Content))
        {
            return BadRequest(new { Message = "Message content cannot be empty." });
        }

        var id = await mediator.Send(new SendMessageCommand(dto));
        return Ok(new { Message = "Message sent and notification triggered", Id = id, MessageId = id });
    }

    // 3. Fetch chat messages for a session
    [HttpGet("messages/{sessionId}")]
    public async Task<IActionResult> GetMessages(Guid sessionId)
    {
        var result = await mediator.Send(new GetChatMessagesQuery(sessionId));
        return Ok(result);
    }

    // 4. Fetch my chat sessions (token direct)
    [HttpGet("my-sessions")]
    [HttpGet("sessions")]
    public async Task<IActionResult> GetMyChatSessions()
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized(new { Message = "User identity could not be verified from token." });
        var result = await mediator.Send(new GetUserChatSessionsQuery(userId));
        return Ok(result);
    }

    // Fetch user's chat sessions by userId
    [HttpGet("sessions/{userId}")]
    public async Task<IActionResult> GetUserSessions(Guid userId)
    {
        var result = await mediator.Send(new GetUserChatSessionsQuery(userId));
        return Ok(result);
    }

    // 5. Mark notification as read
    [HttpPut("notifications/{notificationId}/read")]
    public async Task<IActionResult> MarkNotificationAsRead(Guid notificationId, [FromQuery] Guid? userId)
    {
        var targetUserId = userId.HasValue && userId.Value != Guid.Empty ? userId.Value : GetCurrentUserId();
        var result = await mediator.Send(new MarkNotificationAsReadCommand(notificationId, targetUserId));
        return Ok(new { Message = "Notification marked as read", Success = result });
    }

    // Mark message as read
    [HttpPut("messages/{messageId}/read")]
    public async Task<IActionResult> MarkMessageAsRead(Guid messageId, [FromQuery] Guid? userId)
    {
        var targetUserId = userId.HasValue && userId.Value != Guid.Empty ? userId.Value : GetCurrentUserId();
        var result = await mediator.Send(new MarkMessageAsReadCommand(messageId, targetUserId));
        return Ok(new { Message = "Message marked as read", Success = result });
    }

    // 6. Fetch my notifications (token direct)
    [HttpGet("my-notifications")]
    [HttpGet("notifications")]
    public async Task<IActionResult> GetMyNotifications()
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized(new { Message = "User identity could not be verified from token." });
        var result = await mediator.Send(new GetUserNotificationsQuery(userId));
        return Ok(result);
    }

    // Fetch notifications for a user by userId
    [HttpGet("notifications/{userId}")]
    public async Task<IActionResult> GetUserNotifications(Guid userId)
    {
        var result = await mediator.Send(new GetUserNotificationsQuery(userId));
        return Ok(result);
    }
}
