using System;
using ChatApp.Domain.Enums;

namespace ChatApp.Domain.Entities;

public class RoomMember
{
    public Guid UserId { get; private set; }
    public Guid RoomId { get; private set; }
    public MemberRole Role { get; private set; }
    public DateTime JoinedAtUtc { get; private set; }
    public DateTime? LeftAtUtc { get; private set; }
    public Guid? LastReadMessageId { get; private set; }

    // constructor
    public RoomMember(Guid userId, Guid roomId, MemberRole role)
    {
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

    ////parameterless constructor (constructor rỗng) => không dùng cho business logic, dùng cho Entity Framework Core.
    ////Persistence Ignorance + Encapsulation pattern
    private RoomMember() { }

    // behavior
    public void ChangeRole(MemberRole newRole)
    {
        if (Role == newRole)
            return;

        Role = newRole;
    }

    public void Leave()
    {
        if (LeftAtUtc.HasValue)
            return;

        LeftAtUtc = DateTime.UtcNow;
    }

    public void MarkAsRead(Guid lastReadMessageId)
    {
        if (lastReadMessageId == Guid.Empty)
            throw new ArgumentException("LastReadMessageId cannot be empty.", nameof(lastReadMessageId));

        LastReadMessageId = lastReadMessageId;
    }

    // check owner
    public bool IsOwner()
    {
        return Role == MemberRole.Owner;
    }

    // check admin
    public bool IsAdmin()
    {
        return Role == MemberRole.Admin;
    }

    public bool IsOwnerOrAdmin()
    {
        return Role == MemberRole.Owner || Role == MemberRole.Admin;
    }
}
