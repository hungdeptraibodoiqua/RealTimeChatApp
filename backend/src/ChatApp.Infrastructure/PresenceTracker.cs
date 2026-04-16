using System.Collections.Concurrent;

namespace ChatApp.Infrastructure;

public sealed class PresenceTracker
{
    // Dictionary này lưu userId và danh sách connectionId đang online của user đó.
    // Hub hoặc realtime service sẽ ghi dữ liệu vào đây khi client connect/disconnect để phục vụ nghiệp vụ hiển thị presence.
    private readonly ConcurrentDictionary<Guid, HashSet<string>> _onlineUsers = [];

    public bool UserConnected(Guid userId, string connectionId)
    {
        // Khi client mở một kết nối realtime, userId và connectionId đi từ Hub vào tracker để đánh dấu user online.
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));

        if (string.IsNullOrWhiteSpace(connectionId))
            throw new ArgumentException("ConnectionId cannot be empty.", nameof(connectionId));

        var connections = _onlineUsers.GetOrAdd(userId, _ => []);

        lock (connections)
        {
            connections.Add(connectionId);
            // Trả về true khi đây là connection đầu tiên của user, giúp lớp gọi quyết định có cần broadcast "user online" hay không.
            return connections.Count == 1;
        }
    }

    public bool UserDisconnected(Guid userId, string connectionId)
    {
        // Khi client đóng kết nối realtime, Hub sẽ gọi method này để gỡ connectionId khỏi user tương ứng.
        if (userId == Guid.Empty)
            return false;

        if (string.IsNullOrWhiteSpace(connectionId))
            return false;

        if (!_onlineUsers.TryGetValue(userId, out var connections))
        {
            return false;
        }

        lock (connections)
        {
            connections.Remove(connectionId);

            if (connections.Count > 0)
            {
                // Nếu user vẫn còn tab/device khác đang online thì chưa phát sinh trạng thái offline toàn phần.
                return false;
            }
        }

        _onlineUsers.TryRemove(userId, out _);
        // Trả về true khi user đã hết toàn bộ connection, giúp lớp gọi có thể broadcast "user offline".
        return true;
    }

    public IReadOnlyCollection<Guid> GetOnlineUsers()
    {
        // Method này cung cấp snapshot danh sách user online để controller/hub trả dữ liệu presence cho client nếu cần.
        return _onlineUsers.Keys.OrderBy(userId => userId).ToArray();
    }

    public bool IsOnline(Guid userId)
    {
        // Kiểm tra nhanh trạng thái online của một user để lớp nghiệp vụ khác quyết định cách phản hồi realtime.
        if (userId == Guid.Empty)
            return false;

        return _onlineUsers.ContainsKey(userId);
    }
}
