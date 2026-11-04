using System;
using ChatApp.Domain.Enums;

namespace ChatApp.Domain.Entities;

/// <summary>
/// Đại diện phòng chat, có thể là phòng nhóm hoặc direct tùy theo RoomType.
/// </summary>
public class Room
{
    public Guid Id { get; private set; }
    public string? Name { get; private set; }
    public RoomType Type { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public Guid CreatedByUserId { get; private set; }

    public Room(Guid id, string? name, RoomType type, Guid createdByUserId)
    {
        // Guid.Empty bị chặn để phòng luôn có định danh thật khi liên kết với member/message.
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be empty.", nameof(id));

        // CreatedByUserId là user tạo phòng, cần hợp lệ để audit và phân quyền owner/admin sau này.
        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("CreatedByUserId cannot be empty.", nameof(createdByUserId));

        if (!Enum.IsDefined(typeof(RoomType), type))
            throw new ArgumentException("Invalid RoomType.", nameof(type));

        Id = id;
        Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        Type = type;
        CreatedByUserId = createdByUserId;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public void UpdateName(string? name)
    {
        // Direct room có thể không cần tên, còn group room có thể đặt tên hiển thị.
        Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Kiểm tra user có phải người tạo phòng hay không.
    /// </summary>
    public bool IsCreatedBy(Guid userId)
    {
        if (userId == Guid.Empty)
            return false;

        return CreatedByUserId == userId;
    }
}
