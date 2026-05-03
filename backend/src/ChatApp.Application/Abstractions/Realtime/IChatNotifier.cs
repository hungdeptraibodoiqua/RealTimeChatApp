namespace ChatApp.Application.Abstractions.Realtime;

/// <summary>
/// Contract gửi notification realtime từ Application ra client mà không phụ thuộc trực tiếp SignalR.
/// </summary>
public interface IChatNotifier
{
    /// <summary>
    /// Thông báo user vừa online để các client cập nhật presence.
    /// </summary>
    Task UserOnlineAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thông báo user vừa offline khi không còn connection nào.
    /// </summary>
    Task UserOfflineAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thông báo room có message mới để client reload hoặc append message.
    /// </summary>
    Task MessageCreatedAsync(Guid roomId, Guid messageId, CancellationToken cancellationToken = default);
}
