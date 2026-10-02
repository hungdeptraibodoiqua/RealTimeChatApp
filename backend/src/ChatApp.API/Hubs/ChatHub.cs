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
