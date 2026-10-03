using System.Security.Claims;
using ChatApp.Application.Abstractions.Persistence;
using ChatApp.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.API.Hubs;

/// <summary>
/// Hub SignalR cho tính năng chat realtime, theo dõi presence và typing indicator.
/// Attribute [Authorize] đảm bảo chỉ các user có JWT token hợp lệ mới được mở kết nối.
/// </summary>
[Authorize]
public class ChatHub : Hub
{
    private readonly PresenceTracker _presenceTracker;
    private readonly IRoomRepository _roomRepository;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(
        PresenceTracker presenceTracker,
        IRoomRepository roomRepository,
        ILogger<ChatHub> logger)
    {
        _presenceTracker = presenceTracker;
        _roomRepository = roomRepository;
        _logger = logger;
    }

    /// <summary>
    /// Khi client kết nối SignalR:
    /// 1. Ghi nhận presence online vào PresenceTracker.
    /// 2. Tự động đưa ConnectionId này vào TẤT CẢ các phòng chat của user.
    /// Giải quyết triệt để lỗi mất realtime tin nhắn khi client chưa kịp gọi JoinRoom.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdStr, out var userId))
        {
            var isOnline = _presenceTracker.UserConnected(userId, Context.ConnectionId);
            _logger.LogInformation("User {UserId} kết nối SignalR với ConnectionId {ConnId}", userId, Context.ConnectionId);

            if (isOnline)
            {
                await Clients.Others.SendAsync("UserIsOnline", userId);
            }

            // Tự động gia nhập tất cả các phòng mà user đang là thành viên
            // Tự động gia nhập tất cả các phòng mà user đang là thành viên (chuẩn hóa roomId chữ thường)
            try
            {
                var userRooms = await _roomRepository.GetUserRoomsAsync(userId);
                foreach (var r in userRooms)
                {
                    // Đưa connection vào group phòng chat bằng RoomId chuẩn hóa chữ thường
                    await Groups.AddToGroupAsync(Context.ConnectionId, r.Id.ToString().ToLowerInvariant());
                }
                _logger.LogInformation("Connection {ConnId} đã được tự động đưa vào {Count} phòng chat", Context.ConnectionId, userRooms.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tự động đưa Connection {ConnId} vào các room groups", Context.ConnectionId);
            }
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Tham gia thủ công vào nhóm phòng chat để nhận các tin nhắn và sự kiện realtime của phòng đó.
    /// Chuẩn hóa roomId sang chữ thường để đồng bộ với cơ chế Group của SignalR.
    /// </summary>
    public async Task JoinRoom(string roomId)
    {
        var normalizedRoomId = roomId.ToLowerInvariant().Trim();
        await Groups.AddToGroupAsync(Context.ConnectionId, normalizedRoomId);
        _logger.LogInformation("Connection {ConnId} đã tham gia nhóm {RoomId}", Context.ConnectionId, normalizedRoomId);
    }

    /// <summary>
    /// Rời khỏi nhóm phòng chat khi đóng khung chat.
    /// </summary>
    public async Task LeaveRoom(string roomId)
    {
        var normalizedRoomId = roomId.ToLowerInvariant().Trim();
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, normalizedRoomId);
        _logger.LogInformation("Connection {ConnId} đã rời nhóm {RoomId}", Context.ConnectionId, normalizedRoomId);
    }

    /// <summary>
    /// Phát trạng thái đang gõ phím (Typing Indicator) tới các thành viên khác trong phòng.
    /// Gửi sự kiện 'ReceiveTyping' và 'UserTyping' kèm DisplayName chuẩn từ JWT token.
    /// Sử dụng normalizedRoomId để gửi đúng group và kèm payload chuẩn hóa cho client.
    /// </summary>
    public async Task SendTyping(string roomId, bool isTyping)
    {
        var normalizedRoomId = roomId.ToLowerInvariant().Trim();
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var displayName = Context.User?.FindFirst(ClaimTypes.Name)?.Value
            ?? Context.User?.FindFirst("displayName")?.Value
            ?? Context.User?.FindFirst("username")?.Value
            ?? "Ai đó";

        if (!string.IsNullOrEmpty(userId))
        {
            var payload = new
            {
                RoomId = normalizedRoomId,
                UserId = userId,
                DisplayName = displayName,
                IsTyping = isTyping
            };

            // Phát sự kiện ReceiveTyping và UserTyping tới các client khác trong group đã chuẩn hóa
            await Clients.OthersInGroup(normalizedRoomId).SendAsync("ReceiveTyping", payload);
            await Clients.OthersInGroup(normalizedRoomId).SendAsync("UserTyping", payload);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdStr, out var userId))
        {
            var isOffline = _presenceTracker.UserDisconnected(userId, Context.ConnectionId);
            _logger.LogInformation("User {UserId} ngắt kết nối SignalR", userId);

            if (isOffline)
            {
                await Clients.Others.SendAsync("UserIsOffline", userId);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}
