using System;
using ChatApp.Domain.Enums;

namespace ChatApp.Domain.Entities;

    public enum MemberRole
    {
        Owner = 1,
        Admin = 2,
        Member = 3
    }
    public class RoomMember
    {
        public string UserId { get; private set; }
        public string RoomId { get; private set; }
        public MemberRole Role { get; private set; }
        public DateTime JoinedAt { get; private set; }

        public bool IsMuted { get; private set; }
        public string? LastReadMessageId { get; private set; }

        //constructor
        public RoomMember(string userId, string roomId, MemberRole role)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId cannot be empty.", nameof(userId));

            if (string.IsNullOrWhiteSpace(roomId))
                throw new ArgumentException("RoomId cannot be empty.", nameof(roomId));

            UserId = userId.Trim();
            RoomId = roomId.Trim();
            Role = role;
            JoinedAt = DateTime.UtcNow;
            IsMuted = false;
            LastReadMessageId = null;
        }

        ////parameterless constructor (constructor rỗng) => không dùng cho business logic, dùng cho Entity Framework Core.
        ////Persistence Ignorance + Encapsulation pattern
        private RoomMember() { }

        //behavior
        public void ChangeRole(MemberRole newRole)
        {
            if (Role == newRole)
                return;

            Role = newRole;
        }

        public void Mute()
        {
            if (IsMuted)
                return;

            IsMuted = true;
        }

        public void Unmute()
        {
            if (!IsMuted)
                return;

            IsMuted = false;
        }

        public void MarkAsRead(string lastReadMessageId)
        {
            if (string.IsNullOrWhiteSpace(lastReadMessageId))
                throw new ArgumentException("LastReadMessageId cannot be empty.", nameof(lastReadMessageId));

            LastReadMessageId = lastReadMessageId.Trim();
        }

        //check owner
        public bool IsOwner()
        {
            return Role == MemberRole.Owner;
        }

        //check admin
        public bool IsAdmin()
        {
            return Role == MemberRole.Admin;
        }

        public bool IsOwnerOrAdmin()
        {
            return Role == MemberRole.Owner || Role == MemberRole.Admin;
        }
    }
