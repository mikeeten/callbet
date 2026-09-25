using System.Security.Claims;
using callbet.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace callbet.Infrastructure.Hubs;

[Authorize]
public class ChatHub : Hub<IChatHubClient>
{
    private Guid GetCurrentUserId()
    {
        var user = Context.User;
        if (user == null) return Guid.Empty;

        var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value
            ?? user.FindFirst("id")?.Value
            ?? user.FindFirst("userId")?.Value
            ?? user.FindFirst(ClaimTypes.Sid)?.Value;

        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetCurrentUserId();
        if (userId != Guid.Empty)
        {
            // Add user to their dedicated personal group for targeted notifications and direct alerts
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetCurrentUserId();
        if (userId != Guid.Empty)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    // Join a specific chat session room
    public async Task JoinSession(string sessionId)
    {
        if (Guid.TryParse(sessionId, out var parsedSessionId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, parsedSessionId.ToString());
        }
    }

    // Leave a specific chat session room
    public async Task LeaveSession(string sessionId)
    {
        if (Guid.TryParse(sessionId, out var parsedSessionId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, parsedSessionId.ToString());
        }
    }

    // Broadcast typing indicator to other participants in the session
    public async Task SendTyping(string sessionId, bool isTyping)
    {
        if (Guid.TryParse(sessionId, out var parsedSessionId))
        {
            var userId = GetCurrentUserId();
            await Clients.OthersInGroup(parsedSessionId.ToString()).UserTyping(parsedSessionId, userId, isTyping);
        }
    }
}
