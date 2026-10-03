using ChatApp.API.Hubs;
using ChatApp.Application.Abstractions.Realtime;
using ChatApp.Infrastructure;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.API.Services;

/// <summary>
/// Thực thi IChatNotifier sử dụng IHubContext của SignalR để phát tin nhắn và sự kiện realtime tới các client.
/// Kết nối với: ChatHub.cs (Hub endpoint), Controllers/* (kích hoạt phát sự kiện sau khi lưu DB).
/// </summary>
public class ChatNotifier : IChatNotifier
{
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly PresenceTracker _presenceTracker;

    public ChatNotifier(IHubContext<ChatHub> hubContext, PresenceTracker presenceTracker)
    {
        _hubContext = hubContext;
        _presenceTracker = presenceTracker;
    }

    public async Task UserOnlineAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.All.SendAsync("UserIsOnline", userId, cancellationToken);
    }

    public async Task UserOfflineAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.All.SendAsync("UserIsOffline", userId, cancellationToken);
    }

    public async Task MessageCreatedAsync(Guid roomId, object messageDto, CancellationToken cancellationToken = default)
    {
        // Gửi tin nhắn mới tới toàn bộ client đã JoinRoom vào Group của roomId
        await _hubContext.Clients.Group(roomId.ToString()).SendAsync("ReceiveMessage", messageDto, cancellationToken);
    }

    public async Task MessageRecalledAsync(Guid roomId, Guid messageId, CancellationToken cancellationToken = default)
    {
        // Phát sự kiện thu hồi tin nhắn tới các thành viên trong phòng
        await _hubContext.Clients.Group(roomId.ToString()).SendAsync("MessageRecalled", new { RoomId = roomId, MessageId = messageId }, cancellationToken);
    }

    public async Task MessageEditedAsync(Guid roomId, Guid messageId, string newContent, DateTime editedAtUtc, CancellationToken cancellationToken = default)
    {
        // Phát sự kiện sửa tin nhắn tới các thành viên trong phòng
        await _hubContext.Clients.Group(roomId.ToString()).SendAsync("MessageEdited", new
        {
            RoomId = roomId,
            MessageId = messageId,
            NewContent = newContent,
            EditedAtUtc = editedAtUtc
        }, cancellationToken);
    }

    public async Task MessageDeletedAsync(Guid roomId, Guid messageId, CancellationToken cancellationToken = default)
    {
        // Phát sự kiện xóa tin nhắn tới các thành viên trong phòng
        await _hubContext.Clients.Group(roomId.ToString()).SendAsync("MessageDeleted", new { RoomId = roomId, MessageId = messageId }, cancellationToken);
    }

    public async Task OwnerTransferredAsync(Guid roomId, Guid oldOwnerId, Guid newOwnerId, CancellationToken cancellationToken = default)
    {
        // Thông báo chuyển quyền Owner tới các thành viên trong phòng
        await _hubContext.Clients.Group(roomId.ToString()).SendAsync("OwnerTransferred", new
        {
            RoomId = roomId,
            OldOwnerId = oldOwnerId,
            NewOwnerId = newOwnerId
        }, cancellationToken);
    }

    public async Task UserLeftRoomAsync(Guid roomId, Guid userId, CancellationToken cancellationToken = default)
    {
        // Thông báo thành viên rời phòng
        await _hubContext.Clients.Group(roomId.ToString()).SendAsync("UserLeftRoom", new { RoomId = roomId, UserId = userId }, cancellationToken);
    }

    public async Task MemberRemovedAsync(Guid roomId, Guid userId, CancellationToken cancellationToken = default)
    {
        // Thông báo thành viên bị xóa khỏi phòng
        await _hubContext.Clients.Group(roomId.ToString()).SendAsync("MemberRemoved", new { RoomId = roomId, UserId = userId }, cancellationToken);
    }

    public async Task UserAddedToRoomAsync(Guid userId, object roomDto, CancellationToken cancellationToken = default)
    {
        // 1. Gửi sự kiện AddedToRoom tới riêng user được thêm để UI cập nhật phòng tức thì
        await _hubContext.Clients.User(userId.ToString()).SendAsync("AddedToRoom", roomDto, cancellationToken);

        // 2. Lấy Id của room từ DTO và đưa toàn bộ connection đang mở của user đó vào SignalR Group
        var roomIdProperty = roomDto.GetType().GetProperty("Id")?.GetValue(roomDto)?.ToString();
        if (!string.IsNullOrEmpty(roomIdProperty))
        {
            var connections = _presenceTracker.GetConnections(userId);
            foreach (var connId in connections)
            {
                await _hubContext.Groups.AddToGroupAsync(connId, roomIdProperty, cancellationToken);
            }
        }
    }

    public async Task MemberAddedAsync(Guid roomId, object memberDto, CancellationToken cancellationToken = default)
    {
        // Báo cho các thành viên trong phòng biết có người mới vào
        await _hubContext.Clients.Group(roomId.ToString()).SendAsync("MemberAdded", new { RoomId = roomId, Member = memberDto }, cancellationToken);
    }

    public async Task FriendRequestReceivedAsync(Guid targetUserId, object requestDto, CancellationToken cancellationToken = default)
    {
        // Gửi thông báo lời mời kết bạn tới riêng user nhận
        await _hubContext.Clients.User(targetUserId.ToString()).SendAsync("FriendRequestReceived", requestDto, cancellationToken);
    }

    public async Task FriendRequestAcceptedAsync(Guid requesterUserId, object friendshipDto, CancellationToken cancellationToken = default)
    {
        // Gửi thông báo lời mời được chấp nhận tới người gửi
        await _hubContext.Clients.User(requesterUserId.ToString()).SendAsync("FriendRequestAccepted", friendshipDto, cancellationToken);
    }

    /// <summary>
    /// Phát sự kiện RoomDeleted tới toàn bộ thành viên của phòng chat bị xóa:
    /// - Gửi tới group SignalR của roomId.
    /// - Gửi trực tiếp tới từng UserId của thành viên để đảm bảo nhận được ngay cả khi chưa vào group.
    /// </summary>
    public async Task RoomDeletedAsync(Guid roomId, IEnumerable<Guid>? memberUserIds = null, CancellationToken cancellationToken = default)
    {
        var normalizedRoomId = roomId.ToString().ToLowerInvariant();

        // 1. Phát sự kiện RoomDeleted tới group phòng chat
        await _hubContext.Clients.Group(normalizedRoomId).SendAsync("RoomDeleted", normalizedRoomId, cancellationToken);

        // 2. Đồng thời gửi tới từng UserId của thành viên trong phòng
        if (memberUserIds != null)
        {
            foreach (var userId in memberUserIds)
            {
                await _hubContext.Clients.User(userId.ToString()).SendAsync("RoomDeleted", normalizedRoomId, cancellationToken);
            }
        }
    }
}
