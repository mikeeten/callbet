using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using callbet.Application.DTOs;
using callbet.Application.Chat.Commands;
using callbet.Application.Chat.Queries;

namespace callbet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/chat")]
public class ChatNotificationController(IMediator mediator) : ControllerBase
{
    // 1. Create chat session
    [HttpPost("session")]
    public async Task<IActionResult> CreateChatSession([FromBody] ChatSessionDto dto)
    {
        var id = await mediator.Send(new CreateChatSessionCommand(dto));
        return Ok(new { Message = "Chat session created", Id = id });
    }

    // 2. Send message and trigger notification
    [HttpPost("message")]
    public async Task<IActionResult> SendMessage([FromBody] ChatMessageDto dto)
    {
        var id = await mediator.Send(new SendMessageCommand(dto));
        return Ok(new { Message = "Message sent and notification triggered", Id = id });
    }

    // 3. Fetch chat messages for a session
    [HttpGet("messages/{sessionId}")]
    public async Task<IActionResult> GetMessages(Guid sessionId)
    {
        var result = await mediator.Send(new GetChatMessagesQuery(sessionId));
        return Ok(result);
    }

    // Fetch user's chat sessions
    [HttpGet("sessions/{userId}")]
    public async Task<IActionResult> GetUserSessions(Guid userId)
    {
        var result = await mediator.Send(new GetUserChatSessionsQuery(userId));
        return Ok(result);
    }

    // 4. Mark notification as read
    [HttpPut("notifications/{notificationId}/read")]
    public async Task<IActionResult> MarkNotificationAsRead(Guid notificationId, [FromQuery] Guid userId)
    {
        var result = await mediator.Send(new MarkNotificationAsReadCommand(notificationId, userId));
        return Ok(new { Message = "Notification marked as read", Success = result });
    }

    // Mark message as read
    [HttpPut("messages/{messageId}/read")]
    public async Task<IActionResult> MarkMessageAsRead(Guid messageId, [FromQuery] Guid userId)
    {
        var result = await mediator.Send(new MarkMessageAsReadCommand(messageId, userId));
        return Ok(new { Message = "Message marked as read", Success = result });
    }

    // Fetch notifications for a user
    [HttpGet("notifications/{userId}")]
    public async Task<IActionResult> GetUserNotifications(Guid userId)
    {
        var result = await mediator.Send(new GetUserNotificationsQuery(userId));
        return Ok(result);
    }
}
