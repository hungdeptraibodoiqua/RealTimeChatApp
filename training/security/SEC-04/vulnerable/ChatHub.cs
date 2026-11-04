// GIẢ LẬP CÓ CHỦ ĐÍCH
public async Task SendTyping(string roomId, bool typing)
{
    await Clients.OthersInGroup(roomId).SendAsync("ReceiveTyping", typing);
}
