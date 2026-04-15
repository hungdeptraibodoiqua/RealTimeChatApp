using System;
using ChatApp.Domain.Enums;

namespace ChatApp.Domain.Entities;

    //roomtype
    public enum RoomType
    {
        Direct = 1,
        Group = 2,
        Private = 3
    }

    //constructor
    public class Room
    {
        public string RoomId { get; private set; }
        public string OwnerId { get; private set; }
        public RoomType RoomType { get; private set; }
        public string RoomName { get; private set; }
        public string? RoomPasswordHash { get; private set; } //not all room type need password
        public DateTime CreatedAt { get; private set; }

        public Room(string roomId, string ownerId, RoomType roomType, string roomName, string? roomPasswordHash = null)
        {
            if (string.IsNullOrWhiteSpace(roomId))
                throw new ArgumentException("RoomId cannot be empty.", nameof(roomId));

            if (string.IsNullOrWhiteSpace(ownerId))
                throw new ArgumentException("OwnerId cannot be empty.", nameof(ownerId));

            if (!Enum.IsDefined(typeof(RoomType), roomType))
                throw new ArgumentException("Invalid RoomType.", nameof(roomType));

            if (string.IsNullOrWhiteSpace(roomName))
                throw new ArgumentException("RoomName cannot be empty.", nameof(roomName));

            if (roomType == RoomType.Private && string.IsNullOrWhiteSpace(roomPasswordHash))
                throw new ArgumentException("Private room must have a password.", nameof(roomPasswordHash));

            if (roomType != RoomType.Private && !string.IsNullOrWhiteSpace(roomPasswordHash))
                throw new ArgumentException("Only private rooms can have a password.", nameof(roomPasswordHash));

            RoomId = roomId.Trim();
            OwnerId = ownerId.Trim();
            RoomType = roomType;
            RoomName = roomName.Trim();
            RoomPasswordHash = string.IsNullOrWhiteSpace(roomPasswordHash) ? null : roomPasswordHash.Trim();
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdateRoomName(string roomName)
        {
            if (string.IsNullOrWhiteSpace(roomName))
                throw new ArgumentException("RoomName cannot be empty.", nameof(roomName));

            RoomName = roomName.Trim();
        }

        public void ChangeRoomPassword(string roomPasswordHash)
        {
            if (RoomType != RoomType.Private)
                throw new InvalidOperationException("Only private rooms can have a password.");

            if (string.IsNullOrWhiteSpace(roomPasswordHash))
                throw new ArgumentException("RoomPasswordHash cannot be empty.", nameof(roomPasswordHash));

            RoomPasswordHash = roomPasswordHash.Trim();
        }

        public void RemoveRoomPassword()
        {
            if (RoomType == RoomType.Private)
                throw new InvalidOperationException("Private room cannot remove its password.");

            RoomPasswordHash = null;
        }

        //helper
        public bool IsOwnedBy(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return false;

            return OwnerId == userId.Trim();
        }
    }
