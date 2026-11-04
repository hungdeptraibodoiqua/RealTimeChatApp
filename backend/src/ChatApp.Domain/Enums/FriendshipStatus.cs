namespace ChatApp.Domain.Enums;

// Enum xác định trạng thái của lời mời kết bạn giữa 2 người dùng.
// Kết nối với: Friendship.cs (Domain entity) và FriendsController.cs (API).
public enum FriendshipStatus
{
    Pending = 1,   // Đang chờ chấp nhận
    Accepted = 2,  // Đã chấp nhận kết bạn
    Rejected = 3   // Đã từ chối lời mời
}
