// BẢN VÁ MẪU, đã được áp dụng trong backend/src/ChatApp.API/Hubs/ChatHub.cs
public async Task JoinRoom(string roomId)
{
    var member = await _roomRepository.GetMemberAsync(parsedRoomId, userId);
    if (member == null || member.LeftAtUtc != null) throw new HubException("Forbidden");
    await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
}
