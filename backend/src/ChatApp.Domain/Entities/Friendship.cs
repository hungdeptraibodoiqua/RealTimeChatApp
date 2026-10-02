using ChatApp.Domain.Enums;

namespace ChatApp.Domain.Entities;

/// <summary>
/// Đại diện quan hệ bạn bè hoặc lời mời kết bạn giữa hai User trong hệ thống.
/// Kết nối với: User.cs (Requester và Addressee), FriendshipStatus.cs (trạng thái kết bạn).
/// </summary>
public class Friendship
{
    public Guid Id { get; private set; }
    public Guid RequesterId { get; private set; }
    public Guid AddresseeId { get; private set; }
    public FriendshipStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public Friendship(Guid id, Guid requesterId, Guid addresseeId)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be empty.", nameof(id));

        if (requesterId == Guid.Empty)
            throw new ArgumentException("RequesterId cannot be empty.", nameof(requesterId));

        if (addresseeId == Guid.Empty)
            throw new ArgumentException("AddresseeId cannot be empty.", nameof(addresseeId));

        if (requesterId == addresseeId)
            throw new ArgumentException("Không thể gửi lời mời kết bạn cho chính mình.");

        Id = id;
        RequesterId = requesterId;
        AddresseeId = addresseeId;
        Status = FriendshipStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    // Constructor rỗng dành cho EF Core materialize entity
    private Friendship() { }

    /// <summary>
    /// Chấp nhận lời mời kết bạn.
    /// </summary>
    public void Accept()
    {
        if (Status != FriendshipStatus.Pending)
            throw new InvalidOperationException("Chỉ lời mời đang ở trạng thái Pending mới có thể chấp nhận.");

        Status = FriendshipStatus.Accepted;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Từ chối lời mời kết bạn.
    /// </summary>
    public void Reject()
    {
        if (Status != FriendshipStatus.Pending)
            throw new InvalidOperationException("Chỉ lời mời đang ở trạng thái Pending mới có thể từ chối.");

        Status = FriendshipStatus.Rejected;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
