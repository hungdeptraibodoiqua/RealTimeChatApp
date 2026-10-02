using ChatApp.API.Hubs;
using ChatApp.Application.Abstractions.Realtime;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.API.Services;

/// <summary>
/// Thực thi IChatNotifier sử dụng IHubContext của SignalR để phát tin nhắn và sự kiện realtime tới các client.
/// Kết nối với: ChatHub.cs (Hub endpoint), Controllers/* (kích hoạt phát sự kiện sau khi lưu DB).
/// </summary>
public class ChatNotifier : IChatNotifier
{
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatNotifier(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
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
}
