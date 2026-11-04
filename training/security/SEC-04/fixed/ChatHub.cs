public async Task SendTyping(string roomId, bool typing)
{
    var member = await _roomRepository.GetMemberAsync(parsedRoomId, userId);
    if (member == null || member.LeftAtUtc != null) throw new HubException("Forbidden");
    await Clients.OthersInGroup(roomId).SendAsync("ReceiveTyping", typing);
}
