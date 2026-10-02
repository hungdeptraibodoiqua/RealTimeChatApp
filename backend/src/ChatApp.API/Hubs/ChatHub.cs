using System.Security.Claims;
using ChatApp.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.API.Hubs;

/// <summary>
/// Hub SignalR cho tính năng chat realtime và theo dõi trạng thái online của user.
/// Attribute [Authorize] đảm bảo chỉ các user có JWT token hợp lệ mới được mở kết nối.
/// </summary>
[Authorize]
public class ChatHub : Hub
{
    private readonly PresenceTracker _presenceTracker;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(PresenceTracker presenceTracker, ILogger<ChatHub> logger)
    {
        // Hub inject PresenceTracker từ Infrastructure để lưu trữ trạng thái người dùng online
        _presenceTracker = presenceTracker;
        _logger = logger;
    }

    /// <summary>
    /// Tham gia vào nhóm phòng chat để nhận các tin nhắn và sự kiện realtime của phòng đó.
    /// </summary>
    public async Task JoinRoom(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        _logger.LogInformation("Connection {ConnId} đã tham gia nhóm {RoomId}", Context.ConnectionId, roomId);
    }

    /// <summary>
    /// Rời khỏi nhóm phòng chat khi đổi phòng hoặc đóng khung chat.
    /// </summary>
    public async Task LeaveRoom(string roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        _logger.LogInformation("Connection {ConnId} đã rời nhóm {RoomId}", Context.ConnectionId, roomId);
    }

    /// <summary>
    /// Phát trạng thái đang gõ phím (Typing Indicator) tới các thành viên khác trong phòng.
    /// </summary>
    public async Task SendTyping(string roomId, bool isTyping)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var displayName = Context.User?.FindFirst(ClaimTypes.Name)?.Value
            ?? Context.User?.FindFirst("name")?.Value
            ?? "Ai đó";

        if (!string.IsNullOrEmpty(userId))
        {
            await Clients.OthersInGroup(roomId).SendAsync("UserTyping", new
            {
                RoomId = roomId,
                UserId = userId,
                DisplayName = displayName,
                IsTyping = isTyping
            });
        }
    }

    public override async Task OnConnectedAsync()
    {
        // Đọc userId từ JWT claim đã được JwtBearerHandler giải mã
        var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdStr, out var userId))
        {
            // Ghi nhận connectionId mới của user vào PresenceTracker
            var isOnline = _presenceTracker.UserConnected(userId, Context.ConnectionId);
            _logger.LogInformation("User {UserId} kết nối SignalR với ConnectionId {ConnId}", userId, Context.ConnectionId);

            if (isOnline)
            {
                // Phát thông báo cho tất cả client khác biết user này vừa chuyển sang trạng thái Online
                await Clients.Others.SendAsync("UserIsOnline", userId);
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdStr, out var userId))
        {
            // Gỡ bỏ connectionId khi người dùng đóng tab hoặc ngắt kết nối
            var isOffline = _presenceTracker.UserDisconnected(userId, Context.ConnectionId);
            _logger.LogInformation("User {UserId} ngắt kết nối SignalR", userId);

            if (isOffline)
            {
                // Phát thông báo cho mọi người biết user này đã Offline hoàn toàn
                await Clients.Others.SendAsync("UserIsOffline", userId);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}
