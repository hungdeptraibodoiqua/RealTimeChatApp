using System;
using ChatApp.Domain.Enums;

namespace ChatApp.Domain.Entities;

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
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be empty.", nameof(id));

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
        Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    // helper
    public bool IsCreatedBy(Guid userId)
    {
        if (userId == Guid.Empty)
            return false;

        return CreatedByUserId == userId;
    }
}
