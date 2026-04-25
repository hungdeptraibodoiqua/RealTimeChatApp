using ChatApp.Domain.Enums;

namespace ChatApp.Application.Features.Rooms.Commands.CreateRoom;

public sealed class CreateRoomCommand
{
    public Guid Id { get; init; }

    public string? Name { get; init; }

    public RoomType Type { get; init; }

    public Guid CreatedByUserId { get; init; }
}
