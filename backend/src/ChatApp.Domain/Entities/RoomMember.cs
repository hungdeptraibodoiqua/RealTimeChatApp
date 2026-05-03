using System;
using ChatApp.Domain.Enums;

namespace ChatApp.Domain.Entities;

/// <summary>
/// Đại diện quan hệ một user tham gia một room, gồm role và tiến độ đọc tin nhắn.
/// </summary>
public class RoomMember
{
    public Guid UserId { get; private set; }
    public Guid RoomId { get; private set; }
    public MemberRole Role { get; private set; }
    public DateTime JoinedAtUtc { get; private set; }
    public DateTime? LeftAtUtc { get; private set; }
    public Guid? LastReadMessageId { get; private set; }

    public RoomMember(Guid userId, Guid roomId, MemberRole role)
    {
        // UserId và RoomId tạo thành khóa chính ghép nên không được là Guid.Empty.
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));

        if (roomId == Guid.Empty)
            throw new ArgumentException("RoomId cannot be empty.", nameof(roomId));

        UserId = userId;
        RoomId = roomId;
        Role = role;
        JoinedAtUtc = DateTime.UtcNow;
        LeftAtUtc = null;
        LastReadMessageId = null;
    }

    // Constructor rỗng chỉ dành cho EF Core khi load RoomMember từ database.
    private RoomMember() { }

    /// <summary>
    /// Đổi vai trò thành viên trong phòng, ví dụ Owner/Admin/Member.
    /// </summary>
    public void ChangeRole(MemberRole newRole)
    {
        if (Role == newRole)
            return;

        Role = newRole;
    }

    public void Leave()
    {
        // LeftAtUtc giữ lịch sử membership thay vì xóa cứng record khỏi phòng.
        if (LeftAtUtc.HasValue)
            return;

        LeftAtUtc = DateTime.UtcNow;
    }

    public void MarkAsRead(Guid lastReadMessageId)
    {
        // LastReadMessageId là mốc tùy chọn để client biết user đã đọc tới message nào.
        if (lastReadMessageId == Guid.Empty)
            throw new ArgumentException("LastReadMessageId cannot be empty.", nameof(lastReadMessageId));

        LastReadMessageId = lastReadMessageId;
    }

    /// <summary>
    /// Kiểm tra quyền owner để phục vụ rule quản trị phòng.
    /// </summary>
    public bool IsOwner()
    {
        return Role == MemberRole.Owner;
    }

    /// <summary>
    /// Kiểm tra quyền admin để phục vụ rule quản trị phòng.
    /// </summary>
    public bool IsAdmin()
    {
        return Role == MemberRole.Admin;
    }

    public bool IsOwnerOrAdmin()
    {
        return Role == MemberRole.Owner || Role == MemberRole.Admin;
    }
}
