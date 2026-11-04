namespace ChatApp.Application.Abstractions.Realtime;

/// <summary>
/// Contract gửi notification realtime từ Application / API ra client thông qua SignalR Hub.
/// Tách rời interface này giúp tầng Application không bị phụ thuộc trực tiếp vào package SignalR.
/// </summary>
public interface IChatNotifier
{
    /// <summary>
    /// Thông báo user vừa online để các client cập nhật presence.
    /// </summary>
    Task UserOnlineAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thông báo user vừa offline khi không còn connection nào.
    /// </summary>
    Task UserOfflineAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Phát tin nhắn mới tới toàn bộ thành viên trong phòng chat (Group RoomId).
    /// </summary>
    Task MessageCreatedAsync(Guid roomId, object messageDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Báo cho toàn bộ thành viên phòng biết một tin nhắn vừa bị thu hồi.
    /// </summary>
    Task MessageRecalledAsync(Guid roomId, Guid messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Báo cho toàn bộ thành viên phòng biết một tin nhắn vừa được chỉnh sửa nội dung.
    /// </summary>
    Task MessageEditedAsync(Guid roomId, Guid messageId, string newContent, DateTime editedAtUtc, CancellationToken cancellationToken = default);

    /// <summary>
    /// Báo cho toàn bộ thành viên phòng biết một tin nhắn vừa bị xóa hoàn toàn.
    /// </summary>
    Task MessageDeletedAsync(Guid roomId, Guid messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Báo cho các client trong phòng sự kiện quyền Chủ phòng (Owner) đã được chuyển giao.
    /// </summary>
    Task OwnerTransferredAsync(Guid roomId, Guid oldOwnerId, Guid newOwnerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Báo cho các thành viên trong phòng biết một user vừa rời khỏi phòng.
    /// </summary>
    Task UserLeftRoomAsync(Guid roomId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Báo cho thành viên bị kick ra khỏi phòng hoặc toàn phòng biết danh sách thành viên thay đổi.
    /// </summary>
    Task MemberRemovedAsync(Guid roomId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thông báo cho user cụ thể khi họ vừa được thêm vào một phòng chat mới để cập nhật danh sách tức thì.
    /// </summary>
    Task UserAddedToRoomAsync(Guid userId, object roomDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thông báo cho các thành viên trong phòng biết có người mới vừa được thêm vào.
    /// </summary>
    Task MemberAddedAsync(Guid roomId, object memberDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gửi thông báo tới riêng người nhận khi có lời mời kết bạn mới.
    /// </summary>
    Task FriendRequestReceivedAsync(Guid targetUserId, object requestDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gửi thông báo tới người gửi lời mời khi lời mời được đối phương chấp nhận.
    /// </summary>
    Task FriendRequestAcceptedAsync(Guid requesterUserId, object friendshipDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thông báo cho toàn bộ thành viên trong phòng biết phòng chat đã bị Chủ phòng xóa hoàn toàn.
    /// Phục vụ cập nhật tức thì danh sách phòng và đóng khung chat phía các thành viên realtime.
    /// </summary>
    Task RoomDeletedAsync(Guid roomId, IEnumerable<Guid>? memberUserIds = null, CancellationToken cancellationToken = default);
}
