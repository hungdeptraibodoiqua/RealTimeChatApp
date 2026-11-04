// GIẢ LẬP CÓ CHỦ ĐÍCH: không đưa vào backend thật
public async Task JoinRoom(string roomId)
{
    await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
}
