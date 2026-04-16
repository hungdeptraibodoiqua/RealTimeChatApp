using ChatApp.Domain.Entities;

namespace ChatApp.Application.Features.Rooms.Commands.CreateRoom;

public sealed class CreateRoomCommand
{
    public Guid Id { get; init; }

    public Guid OwnerId { get; init; }

    public RoomType RoomType { get; init; }

    public string RoomName { get; init; } = string.Empty;

    public string? RoomPasswordHash { get; init; }
}
