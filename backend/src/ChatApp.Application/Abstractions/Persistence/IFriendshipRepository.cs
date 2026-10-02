using ChatApp.Domain.Entities;

namespace ChatApp.Application.Abstractions.Persistence;

/// <summary>
/// Contract truy xuất và quản lý lời mời kết bạn và danh sách bạn bè giữa các user.
/// </summary>
public interface IFriendshipRepository
{
    /// <summary>
    /// Tìm quan hệ bạn bè theo Id.
    /// </summary>
    Task<Friendship?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra quan hệ bạn bè giữa hai người dùng cụ thể (bất kể ai là người gửi).
    /// </summary>
    Task<Friendship?> GetFriendshipAsync(Guid user1Id, Guid user2Id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách các quan hệ bạn bè đã được chấp nhận của một user.
    /// </summary>
    Task<List<Friendship>> GetUserFriendsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách các lời mời kết bạn đang chờ user phản hồi.
    /// </summary>
    Task<List<Friendship>> GetPendingRequestsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm lời mời kết bạn mới.
    /// </summary>
    Task AddAsync(Friendship friendship, CancellationToken cancellationToken = default);
}
